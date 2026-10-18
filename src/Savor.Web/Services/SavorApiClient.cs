using System.Text.Json;
using System.Net.Http.Json;
using Savor.Contracts.Recipes;

namespace Savor.Web.Services;

public sealed class SavorApiClient(IConfiguration configuration)
{
    private readonly HttpClient _httpClient = CreateClient(configuration);

    public async Task<RecipeListResponse?> ListRecipesAsync(
        string? tags = null,
        string? ingredients = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(tags))
        {
            foreach (var tag in tags.Split(',', StringSplitOptions.RemoveEmptyEntries |
                         StringSplitOptions.TrimEntries))
            {
                query.Add($"tag={Uri.EscapeDataString(tag)}");
            }
        }
        if (!string.IsNullOrWhiteSpace(ingredients))
        {
            foreach (var ingredient in ingredients.Split(',', StringSplitOptions.RemoveEmptyEntries |
                         StringSplitOptions.TrimEntries))
            {
                query.Add($"ingredient={Uri.EscapeDataString(ingredient)}");
            }
        }

        var path = "/recipes/" + (query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}");
        return await _httpClient.GetFromJsonAsync<RecipeListResponse>(path, cancellationToken);
    }

    public Task<RecipeResponse?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetOrDefaultAsync<RecipeResponse>($"/recipes/{id}", cancellationToken);

    public Task<IReadOnlyList<string>?> ListRecipeTagsAsync(
        CancellationToken cancellationToken = default) =>
        _httpClient.GetFromJsonAsync<IReadOnlyList<string>>("/recipes/tags", cancellationToken);

    public Task<ApiResult<AuthResponse>> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default) =>
        SendAsync<AuthResponse>("/auth/register", HttpMethod.Post, new { email, password }, cancellationToken);

    public Task<ApiResult<AuthResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default) =>
        SendAsync<AuthResponse>("/auth/login", HttpMethod.Post, new { email, password }, cancellationToken);

    public async Task<bool> LogoutAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync("/auth/logout", null, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public Task<CurrentUserResponse?> GetCurrentUserAsync(CancellationToken cancellationToken = default) =>
        GetOrDefaultAsync<CurrentUserResponse>("/me", cancellationToken);

    public async Task<RecipeResponse?> CreateRecipeAsync(
        CreateRecipeRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<RecipeResponse>(
            "/recipes/mine/", HttpMethod.Post, request, cancellationToken);
        return result.Value;
    }

    public async Task<RecipeResponse?> UpdateRecipeAsync(
        Guid id,
        UpdateRecipeRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync<RecipeResponse>(
            $"/recipes/mine/{id}", HttpMethod.Put, request, cancellationToken);
        return result.Value;
    }

    public async Task<bool> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"/recipes/mine/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public Task<RecipeResponse?> GetOwnedRecipeAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        GetOrDefaultAsync<RecipeResponse>($"/recipes/mine/{id}", cancellationToken);

    private async Task<T?> GetOrDefaultAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            : default;
    }

    private async Task<ApiResult<T>> SendAsync<T>(
        string path,
        HttpMethod method,
        object body,
        CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, path)
            {
                Content = JsonContent.Create(body)
            };
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var value = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
                return value is null
                    ? ApiResult<T>.Failure("The server returned an empty response.")
                    : ApiResult<T>.Success(value);
            }

            return ApiResult<T>.Failure(await ReadErrorMessageAsync(response, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return ApiResult<T>.Failure(
                "The Savor API could not be reached. Make sure the API is running and try again.");
        }
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(content))
        {
            return $"The server rejected the request (HTTP {(int)response.StatusCode}).";
        }

        try
        {
            var problem = JsonSerializer.Deserialize<ValidationProblemResponse>(
                content,
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var messages = problem?.Errors?
                .SelectMany(error => error.Value)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToArray();
            if (messages is { Length: > 0 })
            {
                return string.Join(" ", messages);
            }

            if (!string.IsNullOrWhiteSpace(problem?.Detail))
            {
                return problem.Detail;
            }

            if (!string.IsNullOrWhiteSpace(problem?.Title))
            {
                return problem.Title;
            }
        }
        catch (JsonException)
        {
        }

        return $"The server rejected the request (HTTP {(int)response.StatusCode}).";
    }

    private static HttpClient CreateClient(IConfiguration configuration)
    {
        var baseUrl = configuration["SavorApi:BaseUrl"] ?? "http://localhost:5093";
        return new HttpClient(new HttpClientHandler { UseCookies = true })
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
        };
    }
}

public sealed record AuthResponse(Guid UserId, string Email);
public sealed record CurrentUserResponse(string Id, string Email);
public sealed record ApiResult<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null && Value is not null;

    public static ApiResult<T> Success(T value) => new(value, null);
    public static ApiResult<T> Failure(string error) => new(default, error);
}

internal sealed record ValidationProblemResponse(
    string? Title,
    string? Detail,
    Dictionary<string, string[]>? Errors);
