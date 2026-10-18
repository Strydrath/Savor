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
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
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
        Assert.Equal(0, result.Items[0].MatchedIngredientCount);
    }

    [Fact]
    public async Task ListRecipes_ForwardsFiltersAndPagination()
    {
        var response = await _client.GetAsync(
            "/recipes/?tag=quick&ingredient=tomato&ingredient=garlic&page=2&pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeListResponse>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
    }

    [Fact]
    public async Task ListRecipes_SupportsMultipleIngredients()
    {
        var response = await _client.GetAsync(
            "/recipes/?ingredient=tomato&ingredient=garlic");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeListResponse>();

        Assert.NotNull(result);
        Assert.Equal("Creamy Tomato Pasta", result.Items[0].Title);
        Assert.Equal("Garden Omelette", result.Items[1].Title);
    }

    [Fact]
    public async Task ListRecipes_SupportsAllIngredientMode()
    {
        var response = await _client.GetAsync(
            "/recipes/?ingredient=tomato&ingredient=garlic&ingredientMode=All");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeListResponse>();

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ListRecipeTags_ReturnsExistingTags()
    {
        var response = await _client.GetAsync("/recipes/tags");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<string[]>();

        Assert.NotNull(result);
        Assert.Contains("quick", result);
    }

    [Fact]
    public async Task GetRecipe_ReturnsPublicRecipeDetails()
    {
        var response = await _client.GetAsync("/recipes/creamy-tomato-pasta");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<RecipeResponse>();

        Assert.NotNull(result);
        Assert.Equal("Creamy Tomato Pasta", result.Title);
        Assert.Contains(result.Ingredients, ingredient => ingredient.Name == "tomato");
    }

    [Fact]
    public async Task GetRecipe_DoesNotExposePrivateRecipe()
    {
        var response = await _client.GetAsync("/recipes/family-sunday-stew");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRecipe_RedirectsHistoricalSlug()
    {
        var response = await _client.GetAsync("/recipes/old-creamy-tomato-pasta");

        Assert.Equal(HttpStatusCode.PermanentRedirect, response.StatusCode);
        Assert.Equal("/recipes/creamy-tomato-pasta", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task GetRecipe_ReturnsNotFoundForUnknownRecipe()
    {
        var response = await _client.GetAsync("/recipes/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CurrentUser_RequiresAuthentication()
    {
        var response = await _client.GetAsync("/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateRecipe_RequiresAuthentication()
    {
        var response = await _client.PostAsJsonAsync(
            "/recipes/mine/",
            new
            {
                title = "Test recipe",
                description = "Test description"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
        "creamy-tomato-pasta",
        "Creamy Tomato Pasta",
        "A quick pasta with tomato, garlic, and cream.",
        [new RecipeIngredientResponse("tomato", 400, "g")],
        ["quick", "pasta"],
        [new RecipeStepResponse(1, "Cook the pasta.")],
        0,
        []);

    private static readonly RecipeSummaryResponse PartialMatchRecipe = new(
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        "garden-omelette",
        "Garden Omelette",
        "A fluffy omelette with tomato.",
        ["breakfast"],
        0,
        2);

    public Task<RecipeListResponse> ListPublicAsync(
        IReadOnlyList<string> tags,
        IReadOnlyList<string> ingredients,
        IngredientMatchMode ingredientMatchMode,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var matchesFilter = tags.All(tag =>
                string.Equals(tag, "quick", StringComparison.OrdinalIgnoreCase)
                || string.Equals(tag, "pasta", StringComparison.OrdinalIgnoreCase))
            && ingredients.All(ingredient =>
                string.Equals(ingredient, "tomato", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ingredient, "garlic", StringComparison.OrdinalIgnoreCase));
        if (!matchesFilter)
        {
            return Task.FromResult(new RecipeListResponse([], page, pageSize, 0, 0));
        }

        if (ingredientMatchMode == IngredientMatchMode.All && ingredients.Count > 1)
        {
            return Task.FromResult(new RecipeListResponse([], page, pageSize, 0, 0));
        }

        if (ingredients.Count > 1)
        {
            var rankedItems = new RecipeSummaryResponse(
                PublicRecipe.Id,
                PublicRecipe.Slug,
                PublicRecipe.Title,
                PublicRecipe.Description,
                PublicRecipe.Tags,
                ingredients.Count,
                1);
            return Task.FromResult(new RecipeListResponse(
                [rankedItems, PartialMatchRecipe],
                page,
                pageSize,
                2,
                1));
        }

        var item = new RecipeSummaryResponse(
            PublicRecipe.Id,
            PublicRecipe.Slug,
            PublicRecipe.Title,
            PublicRecipe.Description,
            PublicRecipe.Tags,
            ingredients.Count,
            1);

        return Task.FromResult(new RecipeListResponse([item], page, pageSize, 1, 1));
    }

    public Task<RecipeResponse?> GetPublicBySlugAsync(
        string slug,
        IReadOnlyList<string> ingredients,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<RecipeResponse?>(
            slug == PublicRecipe.Slug ? PublicRecipe : null);
    }

    public Task<RecipeResponse?> GetPublicByHistoricalSlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        return Task.FromResult<RecipeResponse?>(
            slug == "old-creamy-tomato-pasta" ? PublicRecipe : null);
    }

    public Task<IReadOnlyList<string>> ListPublicTagsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>(["pasta", "quick"]);
}

internal static class TestData
{
    public static readonly Guid PublicRecipeId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid PrivateRecipeId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");
}
