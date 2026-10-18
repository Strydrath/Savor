using System.ComponentModel.DataAnnotations;

namespace Savor.Contracts.Recipes;

/// <summary>Describes one instruction supplied when creating or updating a recipe.</summary>
public sealed record RecipeStepRequest
{
    [Required, MaxLength(2000)]
    public required string Instruction { get; init; }
}
