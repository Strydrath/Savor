[CmdletBinding()]
param(
    [string]$BaseUrl = "http://localhost:5093",
    [switch]$KeepUser
)

$ErrorActionPreference = "Stop"
$BaseUrl = $BaseUrl.TrimEnd("/")
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$suffix = Get-Date -Format "yyyyMMddHHmmss"
$email = "savor-test-$suffix@example.com"
$password = "StrongPassword123!"
$recipeId = $null

function Convert-ResponseBody {
    param([string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return $null
    }

    try {
        return $Content | ConvertFrom-Json
    }
    catch {
        return $Content
    }
}

function Invoke-Api {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [ValidateSet("Get", "Post", "Put", "Delete")]
        [string]$Method = "Get",

        [object]$Body,

        [int[]]$ExpectedStatus = @(200)
    )

    $request = @{
        Uri = "$BaseUrl$Path"
        Method = $Method
        WebSession = $session
        UseBasicParsing = $true
    }

    if ($null -ne $Body) {
        $request.ContentType = "application/json"
        $request.Body = $Body | ConvertTo-Json -Depth 10
    }

    try {
        $response = Invoke-WebRequest @request
        $statusCode = [int]$response.StatusCode
        $content = $response.Content
    }
    catch {
        if ($null -eq $_.Exception.Response) {
            throw "Could not reach $($request.Uri): $($_.Exception.Message)"
        }

        $response = $_.Exception.Response
        $statusCode = [int]$response.StatusCode
        $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
        $content = $reader.ReadToEnd()
    }

    $parsedBody = Convert-ResponseBody $content

    if ($ExpectedStatus -notcontains $statusCode) {
        $details = if ($parsedBody -is [string]) { $parsedBody } else { $content }
        throw "Unexpected response from $Method $Path. Expected HTTP $($ExpectedStatus -join ', '), got $statusCode.`n$details"
    }

    Write-Host "[PASS] $Method $Path -> $statusCode" -ForegroundColor Green
    return $parsedBody
}

function Assert-Condition {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw "Assertion failed: $Message"
    }
}

try {
    Write-Host "Testing Savor at $BaseUrl" -ForegroundColor Cyan
    Write-Host "Test user: $email"

    $health = Invoke-Api -Path "/health"
    Assert-Condition ($health.status -eq "Healthy") "Health status should be Healthy."

    $recipes = Invoke-Api -Path "/recipes/"
    Assert-Condition ($recipes.totalCount -ge 1) "At least one public seeded recipe should exist."

    $filteredRecipes = Invoke-Api -Path "/recipes/?tag=quick&ingredient=tomato&page=1&pageSize=20"
    Assert-Condition ($filteredRecipes.totalCount -ge 1) "The seeded filtered recipe should be returned."

    $publicRecipeId = $recipes.items[0].id
    $publicRecipe = Invoke-Api -Path "/recipes/$publicRecipeId"
    Assert-Condition ($publicRecipe.id -eq $publicRecipeId) "Public recipe detail ID should match."

    Invoke-Api -Path "/me" -ExpectedStatus @(401) | Out-Null

    $registerBody = @{
        email = $email
        password = $password
    }
    $registered = Invoke-Api -Path "/auth/register" -Method Post -Body $registerBody
    Assert-Condition ($registered.email -eq $email) "Registered email should match."

    $currentUser = Invoke-Api -Path "/me"
    Assert-Condition ($currentUser.email -eq $email) "Current user email should match."

    $recipeBody = @{
        title = "Savor Smoke Test Recipe"
        description = "Created by the Savor API smoke test."
        visibility = "Private"
        ingredients = @(
            @{
                name = "garlic"
                quantity = 2
                unit = "cloves"
            }
        )
        tags = @("test", "smoke")
    }

    $createdRecipe = Invoke-Api -Path "/recipes/mine/" -Method Post -Body $recipeBody -ExpectedStatus @(201)
    $recipeId = $createdRecipe.id
    Assert-Condition ($null -ne $recipeId) "Created recipe should have an ID."

    $ownedRecipe = Invoke-Api -Path "/recipes/mine/$recipeId"
    Assert-Condition ($ownedRecipe.title -eq "Savor Smoke Test Recipe") `
        "Owned recipe should be returned."

    $updateBody = @{
        title = "Updated Savor Smoke Test Recipe"
        description = "Updated by the Savor API smoke test."
        visibility = "Public"
        ingredients = @(
            @{
                name = "garlic"
                quantity = 3
                unit = "cloves"
            }
        )
        tags = @("updated", "smoke")
    }

    $updatedRecipe = Invoke-Api -Path "/recipes/mine/$recipeId" -Method Put -Body $updateBody
    Assert-Condition ($updatedRecipe.title -eq "Updated Savor Smoke Test Recipe") `
        "Updated recipe title should match."

    $publicUpdatedRecipe = Invoke-Api -Path "/recipes/$recipeId"
    Assert-Condition ($publicUpdatedRecipe.id -eq $recipeId) "Published recipe should be publicly readable."

    Invoke-Api -Path "/auth/logout" -Method Post -ExpectedStatus @(204) | Out-Null
    Invoke-Api -Path "/me" -ExpectedStatus @(401) | Out-Null

    $loggedIn = Invoke-Api -Path "/auth/login" -Method Post -Body $registerBody
    Assert-Condition ($loggedIn.email -eq $email) "Login email should match the registered user."

    Invoke-Api -Path "/recipes/mine/$recipeId" | Out-Null

    if (-not $KeepUser) {
        Invoke-Api -Path "/recipes/mine/$recipeId" -Method Delete -ExpectedStatus @(204) | Out-Null
        Invoke-Api -Path "/recipes/mine/$recipeId" -ExpectedStatus @(404) | Out-Null
    }

    if ($KeepUser) {
        Write-Host "Keeping test user and recipe because -KeepUser was specified." -ForegroundColor Yellow
    }

    Write-Host ""
    Write-Host "Savor API smoke test passed." -ForegroundColor Green
}
catch {
    Write-Host ""
    Write-Host "Savor API smoke test failed." -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
