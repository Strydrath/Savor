using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using Savor.Application.Abstractions;
using Savor.Contracts.Recipes;

namespace Savor.Api.Endpoints;

public static class RecipeEndpoints
{
    public static IEndpointRouteBuilder MapRecipeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var recipes = endpoints.MapGroup("/recipes")
            .WithTags("Recipes");

        recipes.MapGet("/", async Task<Ok<RecipeListResponse>> (
                string[]? tag,
                string[]? ingredient,
                int? page,
                int? pageSize,
                IRecipeQueryService service,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await service.ListPublicAsync(
                tag ?? [],
                ingredient ?? [],
                Math.Max(page ?? 1, 1),
                Math.Clamp(pageSize ?? 20, 1, 100),
                cancellationToken)))
            .WithName("ListPublicRecipes")
            .WithSummary("List public recipes")
            .WithDescription("Returns public recipes with optional repeated tag and ingredient filters. Results must match all selected tags and are ranked by the number of requested ingredients they contain.")
            .Produces<RecipeListResponse>();

        recipes.MapGet("/tags", async Task<Ok<IReadOnlyList<string>>> (
                IRecipeQueryService service,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await service.ListPublicTagsAsync(cancellationToken)))
            .WithName("ListRecipeTags")
            .WithSummary("List existing public recipe tags")
            .Produces<IReadOnlyList<string>>();

        recipes.MapGet("/{slug}", async Task<Results<Ok<RecipeResponse>, RedirectHttpResult, NotFound>> (
                string slug,
                IRecipeQueryService service,
                CancellationToken cancellationToken) =>
        {
            var recipe = await service.GetPublicBySlugAsync(slug, cancellationToken);
            if (recipe is not null)
            {
                return TypedResults.Ok(recipe);
            }

            var historicalRecipe = await service.GetPublicByHistoricalSlugAsync(slug, cancellationToken);
            return historicalRecipe is null
                ? TypedResults.NotFound()
                : TypedResults.Redirect($"/recipes/{historicalRecipe.Slug}", permanent: true, preserveMethod: true);
        })
            .WithName("GetPublicRecipe")
            .WithSummary("Get a public recipe")
            .WithDescription("Returns a public recipe by its generated slug. Private recipes are not exposed.")
            .Produces<RecipeResponse>()
            .Produces(StatusCodes.Status308PermanentRedirect)
            .Produces(StatusCodes.Status404NotFound);

        var ownedRecipes = endpoints.MapGroup("/recipes/mine")
            .RequireAuthorization()
            .WithTags("Recipes");

        ownedRecipes.MapGet("/{id:guid}", async Task<Results<Ok<RecipeResponse>, NotFound>> (
                Guid id,
                ClaimsPrincipal user,
                IRecipeCommandService service,
                CancellationToken cancellationToken) =>
        {
            var recipe = await service.GetOwnedByIdAsync(GetUserId(user), id, cancellationToken);
            return recipe is null ? TypedResults.NotFound() : TypedResults.Ok(recipe);
        })
            .WithName("GetOwnedRecipe")
            .WithSummary("Get an owned recipe")
            .Produces<RecipeResponse>()
            .Produces(StatusCodes.Status404NotFound);

        ownedRecipes.MapPost("/", async Task<Created<RecipeResponse>> (
                CreateRecipeRequest request,
                ClaimsPrincipal user,
                IRecipeCommandService service,
                CancellationToken cancellationToken) =>
        {
            var recipe = await service.CreateAsync(GetUserId(user), request, cancellationToken);
            return TypedResults.Created($"/recipes/{recipe.Id}", recipe);
        })
            .WithName("CreateRecipe")
            .WithSummary("Create a recipe")
            .Produces<RecipeResponse>(StatusCodes.Status201Created);

        ownedRecipes.MapPut("/{id:guid}", async Task<Results<Ok<RecipeResponse>, NotFound>> (
                Guid id,
                UpdateRecipeRequest request,
                ClaimsPrincipal user,
                IRecipeCommandService service,
                CancellationToken cancellationToken) =>
        {
            var recipe = await service.UpdateAsync(GetUserId(user), id, request, cancellationToken);
            return recipe is null ? TypedResults.NotFound() : TypedResults.Ok(recipe);
        })
            .WithName("UpdateRecipe")
            .WithSummary("Update an owned recipe")
            .Produces<RecipeResponse>()
            .Produces(StatusCodes.Status404NotFound);

        ownedRecipes.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (
                Guid id,
                ClaimsPrincipal user,
                IRecipeCommandService service,
                CancellationToken cancellationToken) =>
        {
            var deleted = await service.DeleteAsync(GetUserId(user), id, cancellationToken);
            return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
        })
            .WithName("DeleteRecipe")
            .WithSummary("Delete an owned recipe")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("sub");

        return Guid.TryParse(value, out var id)
            ? id
            : throw new InvalidOperationException("The authenticated user ID is invalid.");
    }
}
