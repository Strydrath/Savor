using Microsoft.EntityFrameworkCore;
using Savor.Application.Abstractions;
using Savor.Contracts.Recipes;
using Savor.Domain.Recipes;
using Savor.Infrastructure.Persistence;
using DomainRecipeVisibility = Savor.Domain.Recipes.RecipeVisibility;

namespace Savor.Infrastructure.Recipes;

internal sealed class RecipeCommandService(SavorDbContext dbContext) : IRecipeCommandService
{
    public async Task<RecipeResponse> CreateAsync(
        Guid ownerId,
        CreateRecipeRequest request,
        CancellationToken cancellationToken)
    {
        var recipe = new Recipe(
            ownerId,
            request.Title.Trim(),
            request.Description.Trim(),
            (DomainRecipeVisibility)(int)request.Visibility);
        AddRequestItems(recipe, request.Ingredients, request.Tags);
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(recipe);
    }

    public async Task<RecipeResponse?> UpdateAsync(
        Guid ownerId,
        Guid id,
        UpdateRecipeRequest request,
        CancellationToken cancellationToken)
    {
        var recipe = await dbContext.Recipes
            .Include(item => item.Ingredients)
            .Include(item => item.Tags)
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);

        if (recipe is null)
        {
            return null;
        }

        recipe.Update(
            request.Title.Trim(),
            request.Description.Trim(),
            (DomainRecipeVisibility)(int)request.Visibility);
        AddRequestItems(recipe, request.Ingredients, request.Tags);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(recipe);
    }

    public async Task<bool> DeleteAsync(Guid ownerId, Guid id, CancellationToken cancellationToken)
    {
        var recipe = await dbContext.Recipes
            .SingleOrDefaultAsync(item => item.Id == id && item.OwnerId == ownerId, cancellationToken);

        if (recipe is null)
        {
            return false;
        }

        dbContext.Recipes.Remove(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<RecipeResponse?> GetOwnedByIdAsync(
        Guid ownerId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var recipe = await dbContext.Recipes
            .AsNoTracking()
            .Include(item => item.Ingredients)
            .Include(item => item.Tags)
            .SingleOrDefaultAsync(
                item => item.Id == id && item.OwnerId == ownerId,
                cancellationToken);

        return recipe is null ? null : Map(recipe);
    }

    private static void AddRequestItems(
        Recipe recipe,
        IEnumerable<RecipeIngredientRequest> ingredients,
        IEnumerable<string> tags)
    {
        foreach (var ingredient in ingredients)
        {
            recipe.AddIngredient(ingredient.Name.Trim(), ingredient.Quantity, ingredient.Unit.Trim());
        }

        foreach (var tag in tags.Where(tag => !string.IsNullOrWhiteSpace(tag)))
        {
            recipe.AddTag(tag.Trim());
        }
    }

    private static RecipeResponse Map(Recipe recipe) =>
        new(
            recipe.Id,
            recipe.Title,
            recipe.Description,
            recipe.Ingredients
                .OrderBy(item => item.Name)
                .Select(item => new RecipeIngredientResponse(item.Name, item.Quantity, item.Unit))
                .ToArray(),
            recipe.Tags.Select(item => item.Name).OrderBy(name => name).ToArray());
}
