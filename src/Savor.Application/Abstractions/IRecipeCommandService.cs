using Savor.Contracts.Recipes;
using Savor.Domain.Recipes;

namespace Savor.Application.Abstractions;

public interface IRecipeCommandService
{
    Task<RecipeResponse> CreateAsync(Guid ownerId, CreateRecipeRequest request, CancellationToken cancellationToken);
    Task<RecipeResponse?> UpdateAsync(Guid ownerId, Guid id, UpdateRecipeRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid ownerId, Guid id, CancellationToken cancellationToken);
    Task<RecipeResponse?> GetOwnedByIdAsync(Guid ownerId, Guid id, CancellationToken cancellationToken);
}
