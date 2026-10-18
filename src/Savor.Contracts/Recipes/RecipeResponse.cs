namespace Savor.Contracts.Recipes;

/// <summary>Represents the public details of a recipe.</summary>
public sealed record RecipeResponse(
    Guid Id,
    string Slug,
    string Title,
    string Description,
    IReadOnlyList<RecipeIngredientResponse> Ingredients,
    IReadOnlyList<string> Tags,
    IReadOnlyList<RecipeStepResponse> Steps);
