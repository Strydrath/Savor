using System.Net;
using System.Net.Http.Json;
using Savor.Contracts.Recipes;

namespace Savor.Web.Services;

public sealed class SavorApiClient(IConfiguration configuration)
{
    private readonly HttpClient _httpClient = CreateClient(configuration);

    public async Task<RecipeListResponse?> ListRecipesAsync(
        string? tag = null,
        string? ingredient = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(tag))
        {
            query.Add($"tag={Uri.EscapeDataString(tag)}");
        }
        if (!string.IsNullOrWhiteSpace(ingredient))
        {
            query.Add($"ingredient={Uri.EscapeDataString(ingredient)}");
        }

        var path = "/recipes/" + (query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}");
        return await _httpClient.GetFromJsonAsync<RecipeListResponse>(path, cancellationToken);
    }

    public Task<RecipeResponse?> GetRecipeAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetOrDefaultAsync<RecipeResponse>($"/recipes/{id}", cancellationToken);

    public Task<AuthResponse?> RegisterAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default) =>
        SendAsync<AuthResponse>("/auth/register", HttpMethod.Post, new { email, password }, cancellationToken);

    public Task<AuthResponse?> LoginAsync(
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

    public Task<RecipeResponse?> CreateRecipeAsync(
        CreateRecipeRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<RecipeResponse>("/recipes/mine/", HttpMethod.Post, request, cancellationToken);

    public Task<RecipeResponse?> UpdateRecipeAsync(
        Guid id,
        UpdateRecipeRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync<RecipeResponse>($"/recipes/mine/{id}", HttpMethod.Put, request, cancellationToken);

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

    private async Task<T?> SendAsync<T>(
        string path,
        HttpMethod method,
        object body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path)
        {
            Content = JsonContent.Create(body)
        };
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<T>(cancellationToken)
            : default;
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
