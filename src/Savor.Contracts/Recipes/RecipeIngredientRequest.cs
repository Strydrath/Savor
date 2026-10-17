using System.ComponentModel.DataAnnotations;

namespace Savor.Contracts.Recipes;

/// <summary>Describes an ingredient in a recipe request.</summary>
public sealed record RecipeIngredientRequest
{
    [Required, MaxLength(100)]
    public required string Name { get; init; }

    [Range(0.01, 100000)]
    public decimal Quantity { get; init; }

    [Required, MaxLength(30)]
    public required string Unit { get; init; }
}
