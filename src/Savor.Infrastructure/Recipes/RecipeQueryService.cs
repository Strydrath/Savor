using Microsoft.EntityFrameworkCore;
using Savor.Application.Abstractions;
using Savor.Contracts.Recipes;
using Savor.Domain.Recipes;
using Savor.Infrastructure.Persistence;
using DomainRecipeVisibility = Savor.Domain.Recipes.RecipeVisibility;

namespace Savor.Infrastructure.Recipes;

internal sealed class RecipeQueryService(SavorDbContext dbContext) : IRecipeQueryService
{
    public async Task<RecipeListResponse> ListPublicAsync(
        string? tag,
        string? ingredient,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Recipe> query = dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.Visibility == DomainRecipeVisibility.Public);

        if (!string.IsNullOrWhiteSpace(tag))
        {
            query = query.Where(recipe => recipe.Tags.Any(recipeTag =>
                EF.Functions.ILike(recipeTag.Name, tag.Trim())));
        }

        if (!string.IsNullOrWhiteSpace(ingredient))
        {
            query = query.Where(recipe => recipe.Ingredients.Any(recipeIngredient =>
                EF.Functions.ILike(recipeIngredient.Name, $"%{ingredient.Trim()}%")));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var recipes = await query
            .OrderBy(recipe => recipe.Title)
            .ThenBy(recipe => recipe.Id)
            .Include(recipe => recipe.Tags)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = recipes
            .Select(recipe => new RecipeSummaryResponse(
                recipe.Id,
                recipe.Title,
                recipe.Description,
                recipe.Tags.Select(tagItem => tagItem.Name).OrderBy(name => name).ToArray()))
            .ToArray();

        return new RecipeListResponse(
            items,
            page,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<RecipeResponse?> GetPublicByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var recipe = await dbContext.Recipes
            .AsNoTracking()
            .Where(item => item.Id == id && item.Visibility == DomainRecipeVisibility.Public)
            .Include(item => item.Ingredients)
            .Include(item => item.Tags)
            .SingleOrDefaultAsync(cancellationToken);

        return recipe is null
            ? null
            : new RecipeResponse(
                recipe.Id,
                recipe.Title,
                recipe.Description,
                recipe.Ingredients
                    .OrderBy(ingredient => ingredient.Name)
                    .Select(ingredient => new RecipeIngredientResponse(
                        ingredient.Name,
                        ingredient.Quantity,
                        ingredient.Unit))
                    .ToArray(),
                recipe.Tags.Select(tag => tag.Name).OrderBy(name => name).ToArray());
    }
}
