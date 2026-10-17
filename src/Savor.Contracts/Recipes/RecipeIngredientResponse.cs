namespace Savor.Contracts.Recipes;

/// <summary>Represents an ingredient used by a recipe.</summary>
public sealed record RecipeIngredientResponse(string Name, decimal Quantity, string Unit);
