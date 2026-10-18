using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Savor.Application.Abstractions;
using Savor.Infrastructure.Persistence;
using Savor.Infrastructure.Recipes;
using Savor.Infrastructure.Identity;

namespace Savor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "The 'ConnectionStrings:Database' configuration value is required.");

        services.AddDbContext<SavorDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<SavorDbContext>();
        services.AddScoped<IRecipeQueryService, RecipeQueryService>();
        services.AddScoped<IRecipeCommandService, RecipeCommandService>();
        services.AddScoped<RecipeSlugGenerator>();

        return services;
    }
}
