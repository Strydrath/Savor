using Savor.Domain.Recipes;
using Xunit;

namespace Savor.Domain.Tests;

public sealed class RecipeTests
{
    [Fact]
    public void Recipe_StartsWithNoIngredientsOrTags()
    {
        var recipe = new Recipe(Guid.NewGuid(), "Soup", "A simple soup.", RecipeVisibility.Private);

        Assert.Empty(recipe.Ingredients);
        Assert.Empty(recipe.Tags);
        Assert.Equal(RecipeVisibility.Private, recipe.Visibility);
    }

    [Fact]
    public void Recipe_CanAddIngredientsAndTags()
    {
        var recipe = new Recipe(Guid.NewGuid(), "Soup", "A simple soup.", RecipeVisibility.Public);

        recipe.AddIngredient("carrot", 2, "pieces");
        recipe.AddTag("quick");

        var ingredient = Assert.Single(recipe.Ingredients);
        var tag = Assert.Single(recipe.Tags);
        Assert.Equal("carrot", ingredient.Name);
        Assert.Equal("quick", tag.Name);
    }

    [Fact]
    public void Recipe_UpdateReplacesDetailsAndClearsExistingItems()
    {
        var ownerId = Guid.NewGuid();
        var recipe = new Recipe(ownerId, "Soup", "A simple soup.", RecipeVisibility.Private);
        recipe.AddIngredient("carrot", 2, "pieces");
        recipe.AddTag("quick");
        var recipeId = recipe.Id;

        recipe.Update("Stew", "A slow-cooked stew.", RecipeVisibility.Public);

        Assert.Equal(recipeId, recipe.Id);
        Assert.Equal(ownerId, recipe.OwnerId);
        Assert.Equal("Stew", recipe.Title);
        Assert.Equal("A slow-cooked stew.", recipe.Description);
        Assert.Equal(RecipeVisibility.Public, recipe.Visibility);
        Assert.Empty(recipe.Ingredients);
        Assert.Empty(recipe.Tags);
    }

    [Fact]
    public void Recipe_AddIngredientStoresAllValuesAndRecipeId()
    {
        var recipe = new Recipe(Guid.NewGuid(), "Soup", "A simple soup.", RecipeVisibility.Private);

        recipe.AddIngredient("carrot", 2.5m, "pieces");

        var ingredient = Assert.Single(recipe.Ingredients);
        Assert.Equal(recipe.Id, ingredient.RecipeId);
        Assert.Equal("carrot", ingredient.Name);
        Assert.Equal(2.5m, ingredient.Quantity);
        Assert.Equal("pieces", ingredient.Unit);
    }

    [Fact]
    public void Recipe_AddTagStoresValueAndRecipeId()
    {
        var recipe = new Recipe(Guid.NewGuid(), "Soup", "A simple soup.", RecipeVisibility.Private);

        recipe.AddTag("quick");

        var tag = Assert.Single(recipe.Tags);
        Assert.Equal(recipe.Id, tag.RecipeId);
        Assert.Equal("quick", tag.Name);
    }
}
