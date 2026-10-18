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

    Task<RecipeResponse?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> ListPublicTagsAsync(CancellationToken cancellationToken);
}
