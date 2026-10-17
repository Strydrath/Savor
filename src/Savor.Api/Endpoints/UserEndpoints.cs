using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Savor.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/me", (ClaimsPrincipal user) =>
                TypedResults.Ok(new CurrentUserResponse(
                    user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? user.FindFirstValue("sub")
                        ?? string.Empty,
                    user.FindFirstValue(ClaimTypes.Email)
                        ?? user.FindFirstValue("email")
                        ?? string.Empty)))
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("Get the authenticated user")
            .Produces<CurrentUserResponse>();

        return endpoints;
    }
}

/// <summary>Represents the authenticated user's public identity.</summary>
public sealed record CurrentUserResponse(string Id, string Email);
