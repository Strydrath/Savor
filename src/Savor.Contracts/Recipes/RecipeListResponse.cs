namespace Savor.Contracts.Recipes;

/// <summary>Represents a paginated list of public recipes.</summary>
public sealed record RecipeListResponse(
    IReadOnlyList<RecipeSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
