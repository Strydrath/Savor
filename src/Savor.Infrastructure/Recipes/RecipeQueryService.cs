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
        IReadOnlyList<string> tags,
        IReadOnlyList<string> ingredients,
        IngredientMatchMode ingredientMatchMode,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Recipe> query = dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.Visibility == DomainRecipeVisibility.Public);

        var requestedTags = tags
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var requestedTag in requestedTags)
        {
            query = query.Where(recipe => recipe.Tags.Any(recipeTag =>
                EF.Functions.ILike(recipeTag.Name, requestedTag)));
        }

        var requestedIngredients = ingredients
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var recipesQuery = query
            .Include(recipe => recipe.Tags)
            .Include(recipe => recipe.Ingredients);
        if (requestedIngredients.Length == 0)
        {
            var totalCount = await recipesQuery.CountAsync(cancellationToken);
            var pageRecipes = await recipesQuery
                .OrderBy(recipe => recipe.Title)
                .ThenBy(recipe => recipe.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return CreateListResponse(pageRecipes, [], page, pageSize, totalCount);
        }

        var allRecipes = await recipesQuery.ToListAsync(cancellationToken);
        var rankedRecipes = allRecipes
            .Select(recipe => new
            {
                Recipe = recipe,
                MatchCount = requestedIngredients.Count(requested =>
                    recipe.Ingredients.Any(recipeIngredient =>
                        recipeIngredient.Name.Contains(requested, StringComparison.OrdinalIgnoreCase)))
            })
            .Where(item => ingredientMatchMode == IngredientMatchMode.Any
                ? item.MatchCount > 0
                : item.MatchCount == requestedIngredients.Length)
            .OrderByDescending(item => item.MatchCount)
            .ThenBy(item => item.Recipe.Title)
            .ThenBy(item => item.Recipe.Id)
            .Select(item => item.Recipe)
            .ToArray();

        return CreateListResponse(
            rankedRecipes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray(),
            requestedIngredients,
            page,
            pageSize,
            rankedRecipes.Length);
    }

    private static RecipeListResponse CreateListResponse(
        IReadOnlyList<Recipe> recipes,
        IReadOnlyList<string> requestedIngredients,
        int page,
        int pageSize,
        int totalCount)
    {
        var items = recipes
            .Select(recipe => new RecipeSummaryResponse(
                recipe.Id,
                recipe.Slug,
                recipe.Title,
                recipe.Description,
                recipe.Tags.Select(tagItem => tagItem.Name).OrderBy(name => name).ToArray(),
                MatchedIngredientCount: requestedIngredients.Count(requested =>
                    recipe.Ingredients.Any(recipeIngredient =>
                        recipeIngredient.Name.Contains(requested, StringComparison.OrdinalIgnoreCase))),
                TotalIngredientCount: recipe.Ingredients.Count))
            .ToArray();

        return new RecipeListResponse(
            items,
            page,
            pageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize));
    }

    public async Task<RecipeResponse?> GetPublicBySlugAsync(
        string slug,
        IReadOnlyList<string> ingredients,
        CancellationToken cancellationToken)
    {
        var recipe = await dbContext.Recipes
            .AsNoTracking()
            .Where(item => item.Slug == slug && item.Visibility == DomainRecipeVisibility.Public)
            .Include(item => item.Ingredients)
            .Include(item => item.Tags)
            .Include(item => item.Steps)
            .SingleOrDefaultAsync(cancellationToken);

        return recipe is null ? null : MapRecipe(recipe, ingredients);
    }

    public async Task<RecipeResponse?> GetPublicByHistoricalSlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        var recipeId = await dbContext.RecipeSlugHistory
            .Where(history => history.Slug == slug)
            .Select(history => (Guid?)history.RecipeId)
            .SingleOrDefaultAsync(cancellationToken);

        if (!recipeId.HasValue)
        {
            return null;
        }

        var recipe = await dbContext.Recipes
            .AsNoTracking()
            .Where(item => item.Id == recipeId.Value && item.Visibility == DomainRecipeVisibility.Public)
            .Include(item => item.Ingredients)
            .Include(item => item.Tags)
            .Include(item => item.Steps)
            .SingleOrDefaultAsync(cancellationToken);

        return recipe is null
            ? null
            : MapRecipe(recipe, []);
    }

    private static RecipeResponse MapRecipe(Recipe recipe, IReadOnlyList<string> ingredients)
    {
        var requestedIngredients = ingredients
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var matchedIngredients = recipe.Ingredients
            .Where(recipeIngredient => requestedIngredients.Any(requested =>
                recipeIngredient.Name.Contains(requested, StringComparison.OrdinalIgnoreCase)))
            .Select(recipeIngredient => recipeIngredient.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new RecipeResponse(
            recipe.Id,
            recipe.Slug,
            recipe.Title,
            recipe.Description,
            recipe.Ingredients
                .OrderBy(ingredient => ingredient.Name)
                .Select(ingredient => new RecipeIngredientResponse(
                    ingredient.Name,
                    ingredient.Quantity,
                    ingredient.Unit))
                .ToArray(),
            recipe.Tags.Select(tag => tag.Name).OrderBy(name => name).ToArray(),
            recipe.Steps
                .OrderBy(step => step.StepNumber)
                .Select(step => new RecipeStepResponse(step.StepNumber, step.Instruction))
                .ToArray(),
            matchedIngredients.Length,
            recipe.Ingredients
                .Where(recipeIngredient => !matchedIngredients.Contains(
                    recipeIngredient.Name,
                    StringComparer.OrdinalIgnoreCase))
                .Select(recipeIngredient => recipeIngredient.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToArray());
    }

    public async Task<IReadOnlyList<string>> ListPublicTagsAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.RecipeTags
            .Where(tag => dbContext.Recipes.Any(recipe =>
                recipe.Id == tag.RecipeId &&
                recipe.Visibility == DomainRecipeVisibility.Public))
            .GroupBy(tag => tag.Name)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => group.Key)
            .ToListAsync(cancellationToken);
    }
}
