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
}
