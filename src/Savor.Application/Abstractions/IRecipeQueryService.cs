using Savor.Contracts.Recipes;

namespace Savor.Application.Abstractions;

public interface IRecipeQueryService
{
    Task<RecipeListResponse> ListPublicAsync(
        string? tag,
        string? ingredient,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<RecipeResponse?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken);
}
