using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Savor.Application.Abstractions;
using Savor.Infrastructure.Persistence;
using Savor.Infrastructure.Recipes;

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
        services.AddScoped<IRecipeQueryService, RecipeQueryService>();

        return services;
    }
}
