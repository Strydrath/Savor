using Microsoft.AspNetCore.Http.HttpResults;
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
                string? tag,
                string? ingredient,
                int? page,
                int? pageSize,
                IRecipeQueryService service,
                CancellationToken cancellationToken) =>
            TypedResults.Ok(await service.ListPublicAsync(
                tag,
                ingredient,
                Math.Max(page ?? 1, 1),
                Math.Clamp(pageSize ?? 20, 1, 100),
                cancellationToken)))
            .WithName("ListPublicRecipes")
            .WithSummary("List public recipes")
            .WithDescription("Returns public recipes with optional tag and ingredient filters.")
            .Produces<RecipeListResponse>();

        recipes.MapGet("/{id:guid}", async Task<Results<Ok<RecipeResponse>, NotFound>> (
                Guid id,
                IRecipeQueryService service,
                CancellationToken cancellationToken) =>
        {
            var recipe = await service.GetPublicByIdAsync(id, cancellationToken);
            return recipe is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(recipe);
        })
            .WithName("GetPublicRecipe")
            .WithSummary("Get a public recipe")
            .WithDescription("Returns a public recipe by ID. Private recipes are not exposed.")
            .Produces<RecipeResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }
}
