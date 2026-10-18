# Savor

Recipe discovery and pantry-matching app for a small group of users. Browse public recipes, keep private ones, filter by tags and ingredients, and request a random recipe.

## Stack

- ASP.NET Core Web API
- PostgreSQL + Entity Framework Core
- Clean architecture: Domain, Application, Infrastructure, Contracts, Api
- Blazor Web and .NET MAUI planned for later vertical slices
- Blazor Web client for recipe discovery, authentication, and owned recipes

## Solution layout

```
src/
  Savor.Api/              # HTTP composition root
  Savor.Application/      # use cases and application policies
  Savor.Domain/           # entities and domain rules
  Savor.Infrastructure/   # EF Core, PostgreSQL, adapters
  Savor.Contracts/        # API DTOs and shared contracts
```

## Prerequisites

- .NET SDK 10
- Docker (optional, for local PostgreSQL)

## Getting started

1. Start PostgreSQL:

```powershell
docker compose -f deploy/docker/docker-compose.yml up -d
```

2. Set the local connection string for the current PowerShell session:

```powershell
$env:ConnectionStrings__Database = "Host=localhost;Port=55432;Database=savor;Username=savor;Password=savor"
```

The password above must match `POSTGRES_PASSWORD` in
[docker-compose.yml](deploy/docker/docker-compose.yml). The application does
not store a database password in tracked JSON configuration.

3. Apply the database migration and seed data:

```powershell
dotnet ef database update `
  --project src/Savor.Infrastructure `
  --startup-project src/Savor.Api
```

The development connection string matches the Docker Compose database. To use a
different database, set `ConnectionStrings__Database` before starting the API.

4. Run the API:

```powershell
dotnet run --project src/Savor.Api
```

5. Open `/health` or the OpenAPI document in Development.

The HTTP launch profile uses `http://localhost:5093`. The HTTPS profile uses
`https://localhost:7249` and `http://localhost:5093`.

## Running the web UI

Start the API first:

```powershell
dotnet run --project src/Savor.Api
```

Then start the Blazor web client in another PowerShell window:

```powershell
dotnet run --project src/Savor.Web
```

Open the URL printed for `Savor.Web`. The client is configured to call the API
at `http://localhost:5093` through `SavorApi:BaseUrl` in
[appsettings.json](src/Savor.Web/appsettings.json). The first UI slice includes
public recipe browsing and filtering, recipe details, registration, login, and
owned recipe create/update/delete screens.

## Configuration

Connection string key: `ConnectionStrings:Database`.

The checked-in Development configuration is for local Docker Compose only.
Production deployments should provide the connection string through an
environment variable or another secret configuration provider.

## Testing the current functionality

### Health and OpenAPI

With the API running:

```powershell
Invoke-RestMethod http://localhost:5093/health
Invoke-WebRequest http://localhost:5093/openapi/v1.json
```

The health endpoint should return HTTP 200 and a response with `status` equal
to `Healthy`. The OpenAPI request should return HTTP 200 in Development.

### Public recipe discovery

Public recipe URLs use readable slugs generated automatically from the recipe
title. For example, `Creamy Tomato Pasta` becomes `creamy-tomato-pasta`; if
that slug already exists, the next recipes use `-2`, `-3`, and so on. Slugs
are never entered manually. Owner management endpoints continue to use the
internal recipe ID. When a recipe is renamed, its previous slug is retained
and permanently redirects to the current public URL.

List all public recipes:

```powershell
Invoke-RestMethod http://localhost:5093/recipes/
```

Filter by tag or ingredient and paginate the result:

```powershell
Invoke-RestMethod 'http://localhost:5093/recipes/?tag=quick'
Invoke-RestMethod 'http://localhost:5093/recipes/?ingredient=tomato&page=1&pageSize=20'
```

Search with multiple ingredients by separating them with commas in the web UI,
or by repeating the `ingredient` query parameter in API requests:

```powershell
Invoke-RestMethod 'http://localhost:5093/recipes/?ingredient=tomato&ingredient=garlic'
```

By default, recipes containing all requested ingredients appear first,
followed by recipes containing fewer of the requested ingredients. Results
containing at least one requested ingredient are included. Use
`ingredientMode=All` to exclude recipes that do not contain every requested
ingredient:

```powershell
Invoke-RestMethod 'http://localhost:5093/recipes/?ingredient=tomato&ingredient=garlic&ingredientMode=All'
```

Recipe list responses include `matchedIngredientCount` and
`totalIngredientCount`. When opening a recipe from a pantry-filtered list, the
selected ingredients are carried to the detail page, which shows the matching
count and missing ingredient names.

Recipe tags can be selected from the first three most-used public tags, or
searched by typing. Multiple tags can be selected in the web UI and sent to
the API by repeating the `tag` query parameter:

```powershell
Invoke-RestMethod 'http://localhost:5093/recipes/?tag=quick&tag=pasta'
```

Get the seeded public recipe:

```powershell
Invoke-RestMethod `
  http://localhost:5093/recipes/creamy-tomato-pasta
```

The seeded private recipe must not be exposed and should return HTTP 404:

```powershell
try {
    Invoke-WebRequest `
      http://localhost:5093/recipes/22222222-2222-2222-2222-222222222222 `
      -UseBasicParsing
} catch {
    $_.Exception.Response.StatusCode
}
```

The same requests are available in
[Savor.http](src/Savor.Api/Savor.http) and can be run from the VS Code REST
Client extension or another `.http`-file compatible client.

### Authentication and owned recipes

The API uses ASP.NET Core Identity with cookie authentication for the current
development slice. Register and log in first:

```powershell
Invoke-WebRequest `
  http://localhost:5093/auth/register `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"cook@example.com","password":"StrongPassword123!"}' `
  -SessionVariable session

Invoke-WebRequest `
  http://localhost:5093/auth/login `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"cook@example.com","password":"StrongPassword123!"}' `
  -WebSession $session
```

Use the same session to call the authenticated endpoints:

```powershell
Invoke-RestMethod http://localhost:5093/me -WebSession $session

$recipe = Invoke-RestMethod `
  http://localhost:5093/recipes/mine/ `
  -Method Post `
  -ContentType "application/json" `
  -WebSession $session `
  -Body '{"title":"My Garlic Soup","description":"A private recipe.","visibility":"Private","ingredients":[{"name":"garlic","quantity":2,"unit":"cloves"}],"tags":["quick"]}'
```

Owned recipe endpoints:

```text
GET    /recipes/mine/{id}
POST   /recipes/mine/
PUT    /recipes/mine/{id}
DELETE /recipes/mine/{id}
POST   /auth/logout
GET    /me
```

Anonymous users can still browse public recipes. Private recipes are only
returned through the owner-scoped endpoint for the authenticated owner.

### Build and tests

Build the solution:

```powershell
dotnet build Savor.slnx --configuration Release
```

Run the test suite:

```powershell
dotnet test Savor.slnx --configuration Release
```

The repository includes domain tests and API endpoint tests. The API tests use
a deterministic query-service test double, so they do not require PostgreSQL.

Run the end-to-end API smoke test from the repository root after starting the
database, applying migrations, and running the API:

```powershell
.\scripts\test-api.ps1
```

Use a different API URL when needed:

```powershell
.\scripts\test-api.ps1 -BaseUrl "http://localhost:5093"
```

The script creates a timestamped test user, exercises public discovery,
registration, login-session behavior, `/me`, private recipe creation, update,
publication, and logout. It reports expected non-success responses such as
anonymous `401 Unauthorized` as passing checks. Add `-KeepUser` to retain the
created test data for inspection.

## Linux server deployment

For a production-style Linux deployment using Docker PostgreSQL, systemd, and
an optional Nginx reverse proxy, see
[docs/linux-server-setup.md](docs/linux-server-setup.md).

## Status

Public recipe discovery is implemented with PostgreSQL persistence, seeded
development data, filtering, pagination, and OpenAPI documentation.
Authentication, recipe authoring, web, and mobile clients are not implemented
yet.
