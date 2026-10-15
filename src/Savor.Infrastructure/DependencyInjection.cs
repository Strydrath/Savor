using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Savor.Infrastructure.Persistence;

namespace Savor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? "Host=localhost;Port=5432;Database=savor;Username=savor;Password=savor";

        services.AddDbContext<SavorDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
