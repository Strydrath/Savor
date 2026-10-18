namespace Savor.Contracts.Recipes;

/// <summary>Represents a recipe in a public recipe list.</summary>
public sealed record RecipeSummaryResponse(
    Guid Id,
    string Slug,
    string Title,
    string Description,
    IReadOnlyList<string> Tags);
