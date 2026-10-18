using System.Net;
using System.Net.Http.Json;
using Savor.Contracts.Recipes;
using Xunit;

namespace Savor.IntegrationTests;

public sealed class RecipeIntegrationTests(PostgresIntegrationFixture fixture)
    : IClassFixture<PostgresIntegrationFixture>
{
    [Fact]
    public async Task RegistrationCreatesAuthenticatedSession()
    {
        using var client = fixture.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.com";

        var register = await client.PostAsJsonAsync("/auth/register", new
        {
            email,
            password = "StrongPassword123!"
        });

        Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        var me = await client.GetAsync("/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task PrivateRecipeIsVisibleToOwnerButNotPublicly()
    {
        using var client = await RegisterAsync();
        var create = await CreateRecipeAsync(client, "Private integration recipe", RecipeVisibility.Private);
        var recipe = await create.Content.ReadFromJsonAsync<RecipeResponse>();

        Assert.NotNull(recipe);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/recipes/{recipe.Slug}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/recipes/mine/{recipe.Id}")).StatusCode);
    }

    [Fact]
    public async Task DifferentUsersCannotAccessEachOthersRecipes()
    {
        using var owner = await RegisterAsync();
        var create = await CreateRecipeAsync(owner, "Owner isolation recipe", RecipeVisibility.Private);
        var recipe = await create.Content.ReadFromJsonAsync<RecipeResponse>();
        using var otherUser = await RegisterAsync();

        Assert.NotNull(recipe);
        Assert.Equal(HttpStatusCode.NotFound, (await otherUser.GetAsync($"/recipes/mine/{recipe.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await otherUser.DeleteAsync($"/recipes/mine/{recipe.Id}")).StatusCode);
    }

    [Fact]
    public async Task DuplicateTitlesGetUniqueSlugsAndHistoricalSlugsRedirect()
    {
        using var client = await RegisterAsync();
        var firstResponse = await CreateRecipeAsync(client, "Slug history integration", RecipeVisibility.Public);
        var secondResponse = await CreateRecipeAsync(client, "Slug history integration", RecipeVisibility.Public);
        var first = await firstResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        var second = await secondResponse.Content.ReadFromJsonAsync<RecipeResponse>();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal("slug-history-integration", first.Slug);
        Assert.Equal("slug-history-integration-2", second.Slug);

        var update = await client.PutAsJsonAsync($"/recipes/mine/{first.Id}", new
        {
            title = "Renamed slug history integration",
            description = "Renamed integration recipe.",
            visibility = RecipeVisibility.Public,
            ingredients = Array.Empty<object>(),
            tags = Array.Empty<string>(),
            steps = Array.Empty<object>()
        });
        var updated = await update.Content.ReadFromJsonAsync<RecipeResponse>();

        Assert.NotNull(updated);
        Assert.Equal(HttpStatusCode.PermanentRedirect,
            (await client.GetAsync($"/recipes/{first.Slug}")).StatusCode);
        Assert.Equal($"/recipes/{updated.Slug}",
            (await client.GetAsync($"/recipes/{first.Slug}")).Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task PantryModesUseRealDatabaseData()
    {
        using var client = await RegisterAsync();
        await CreateRecipeAsync(
            client,
            "Pantry all match integration",
            RecipeVisibility.Public,
            ["tomato", "garlic"]);
        await CreateRecipeAsync(
            client,
            "Pantry partial match integration",
            RecipeVisibility.Public,
            ["tomato"]);

        var any = await client.GetFromJsonAsync<RecipeListResponse>(
            "/recipes/?ingredient=tomato&ingredient=garlic&ingredientMode=Any");
        var all = await client.GetFromJsonAsync<RecipeListResponse>(
            "/recipes/?ingredient=tomato&ingredient=garlic&ingredientMode=All");

        Assert.NotNull(any);
        Assert.NotNull(all);
        Assert.True(any.TotalCount >= 2);
        Assert.Contains(any.Items, item => item.MatchedIngredientCount == 2);
        Assert.Contains(all.Items, item => item.Title == "Pantry all match integration");
        Assert.DoesNotContain(all.Items, item => item.Title == "Pantry partial match integration");
        Assert.All(all.Items, item => Assert.Equal(2, item.MatchedIngredientCount));
    }

    private async Task<HttpClient> RegisterAsync()
    {
        var client = fixture.CreateClient();
        await client.PostAsJsonAsync("/auth/register", new
        {
            email = $"{Guid.NewGuid():N}@example.com",
            password = "StrongPassword123!"
        });
        return client;
    }

    private static async Task<HttpResponseMessage> CreateRecipeAsync(
        HttpClient client,
        string title,
        RecipeVisibility visibility,
        IReadOnlyList<string>? ingredientNames = null)
    {
        var ingredients = (ingredientNames ?? []).Select(name => new
        {
            name,
            quantity = 1,
            unit = "piece"
        });
        return await client.PostAsJsonAsync("/recipes/mine/", new
        {
            title,
            description = "Integration test recipe.",
            visibility,
            ingredients,
            tags = Array.Empty<string>(),
            steps = Array.Empty<object>()
        });
    }
}
