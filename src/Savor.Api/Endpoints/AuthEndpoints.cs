using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Savor.Infrastructure.Identity;

namespace Savor.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var auth = endpoints.MapGroup("/auth").WithTags("Authentication");

        auth.MapPost("/register", async Task<Results<Ok<AuthResponse>, ValidationProblem>> (
                RegisterRequest request,
                UserManager<ApplicationUser> userManager,
                HttpContext httpContext) =>
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email
            };
            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return TypedResults.ValidationProblem(
                    result.Errors.ToDictionary(error => error.Code, error => new[] { error.Description }));
            }

            await SignInAsync(httpContext, user);
            return TypedResults.Ok(new AuthResponse(user.Id, user.Email!));
        })
            .WithName("Register")
            .WithSummary("Register a user")
            .Produces<AuthResponse>()
            .ProducesValidationProblem();

        auth.MapPost("/login", async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> (
                LoginRequest request,
                UserManager<ApplicationUser> userManager,
                HttpContext httpContext) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            {
                return TypedResults.Unauthorized();
            }

            await SignInAsync(httpContext, user);
            return TypedResults.Ok(new AuthResponse(user.Id, user.Email!));
        })
            .WithName("Login")
            .WithSummary("Log in a user")
            .Produces<AuthResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        auth.MapPost("/logout", async (HttpContext httpContext) =>
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return TypedResults.NoContent();
        })
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Log out the current user")
            .Produces(StatusCodes.Status204NoContent);

        return endpoints;
    }

    private static Task SignInAsync(HttpContext httpContext, ApplicationUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}

/// <summary>Payload for registering a user.</summary>
public sealed record RegisterRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required, MinLength(8)]
    public required string Password { get; init; }
}

/// <summary>Payload for logging in a user.</summary>
public sealed record LoginRequest
{
    [Required, EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}

/// <summary>Represents a successful authentication result.</summary>
public sealed record AuthResponse(Guid UserId, string Email);
