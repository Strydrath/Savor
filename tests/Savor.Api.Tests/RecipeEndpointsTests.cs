using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Savor.Application.Abstractions;
using Savor.Contracts.Recipes;
using Xunit;

namespace Savor.Api.Tests;

public sealed class RecipeEndpointsTests : IClassFixture<SavorApiFactory>
{
    private readonly HttpClient _client;

    public RecipeEndpointsTests(SavorApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ListRecipes_ReturnsPublicRecipes()
    {
        var response = await _client.GetAsync("/recipes/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeListResponse>();

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Creamy Tomato Pasta", result.Items[0].Title);
    }

    [Fact]
    public async Task ListRecipes_ForwardsFiltersAndPagination()
    {
        var response = await _client.GetAsync("/recipes/?tag=quick&ingredient=tomato&page=2&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeListResponse>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
    }

    [Fact]
    public async Task GetRecipe_ReturnsPublicRecipeDetails()
    {
        var response = await _client.GetAsync($"/recipes/{TestData.PublicRecipeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        Assert.NotNull(result);
        Assert.Equal("Creamy Tomato Pasta", result.Title);
        Assert.Contains(result.Ingredients, ingredient => ingredient.Name == "tomato");
    }

    [Fact]
    public async Task GetRecipe_DoesNotExposePrivateRecipe()
    {
        var response = await _client.GetAsync($"/recipes/{TestData.PrivateRecipeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRecipe_ReturnsNotFoundForUnknownRecipe()
    {
        var response = await _client.GetAsync($"/recipes/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public sealed class SavorApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(
            "ConnectionStrings:Database",
            "Host=127.0.0.1;Port=1;Database=test;Username=test;Password=test");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(service =>
                service.ServiceType == typeof(IRecipeQueryService));
            services.Remove(descriptor);
            services.AddSingleton<IRecipeQueryService, TestRecipeQueryService>();
        });
    }
}

internal sealed class TestRecipeQueryService : IRecipeQueryService
{
    private static readonly RecipeResponse PublicRecipe = new(
        TestData.PublicRecipeId,
        "Creamy Tomato Pasta",
        "A quick pasta with tomato, garlic, and cream.",
        [new RecipeIngredientResponse("tomato", 400, "g")],
        ["quick", "pasta"]);

    public Task<RecipeListResponse> ListPublicAsync(
        string? tag,
        string? ingredient,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var matchesFilter = (string.IsNullOrWhiteSpace(tag)
                || string.Equals(tag, "quick", StringComparison.OrdinalIgnoreCase))
            && (string.IsNullOrWhiteSpace(ingredient)
                || string.Equals(ingredient, "tomato", StringComparison.OrdinalIgnoreCase));
        if (!matchesFilter)
        {
            return Task.FromResult(new RecipeListResponse([], page, pageSize, 0, 0));
        }

        var item = new RecipeSummaryResponse(
            PublicRecipe.Id,
            PublicRecipe.Title,
            PublicRecipe.Description,
            PublicRecipe.Tags);

        return Task.FromResult(new RecipeListResponse([item], page, pageSize, 1, 1));
    }

    public Task<RecipeResponse?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult<RecipeResponse?>(
            id == TestData.PublicRecipeId ? PublicRecipe : null);
    }
}

internal static class TestData
{
    public static readonly Guid PublicRecipeId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid PrivateRecipeId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");
}
