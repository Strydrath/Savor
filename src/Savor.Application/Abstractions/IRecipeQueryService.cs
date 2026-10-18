using Savor.Contracts.Recipes;

namespace Savor.Application.Abstractions;

public interface IRecipeQueryService
{
    Task<RecipeListResponse> ListPublicAsync(
        IReadOnlyList<string> tags,
        IReadOnlyList<string> ingredients,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<RecipeResponse?> GetPublicBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<RecipeResponse?> GetPublicByHistoricalSlugAsync(
        string slug,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> ListPublicTagsAsync(CancellationToken cancellationToken);
}
