namespace Savor.Domain.Recipes;

public sealed class RecipeSlugHistory
{
    private RecipeSlugHistory()
    {
    }

    public RecipeSlugHistory(string slug, Guid recipeId)
    {
        Slug = slug;
        RecipeId = recipeId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Slug { get; private set; } = string.Empty;
    public Guid RecipeId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
