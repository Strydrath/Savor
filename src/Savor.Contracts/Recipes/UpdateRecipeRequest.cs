using System.ComponentModel.DataAnnotations;
namespace Savor.Contracts.Recipes;

/// <summary>Payload for replacing an owned recipe.</summary>
public sealed record UpdateRecipeRequest
{
    [Required, MaxLength(200)]
    public required string Title { get; init; }

    [Required, MaxLength(4000)]
    public required string Description { get; init; }

    public RecipeVisibility Visibility { get; init; } = RecipeVisibility.Private;

    public IReadOnlyList<RecipeIngredientRequest> Ingredients { get; init; } = [];

    public IReadOnlyList<string> Tags { get; init; } = [];
}
