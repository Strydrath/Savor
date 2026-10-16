namespace Savor.Domain.Recipes;

public sealed class RecipeTag
{
    private RecipeTag()
    {
    }

    internal RecipeTag(Guid recipeId, string name)
    {
        RecipeId = recipeId;
        Name = name;
    }

    public Guid RecipeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
}
