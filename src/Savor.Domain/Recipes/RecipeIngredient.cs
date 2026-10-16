namespace Savor.Domain.Recipes;

public sealed class RecipeIngredient
{
    private RecipeIngredient()
    {
    }

    internal RecipeIngredient(Guid recipeId, string name, decimal quantity, string unit)
    {
        RecipeId = recipeId;
        Name = name;
        Quantity = quantity;
        Unit = unit;
    }

    public Guid RecipeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public string Unit { get; private set; } = string.Empty;
}
