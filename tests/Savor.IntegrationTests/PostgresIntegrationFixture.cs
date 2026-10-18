using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Savor.Infrastructure.Persistence;

namespace Savor.IntegrationTests;

public sealed class PostgresIntegrationFixture : IDisposable
{
    private readonly string _adminConnectionString;
    private readonly string _databaseName;

    public PostgresIntegrationFixture()
    {
        var configuredConnectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=55432;Database=savor;Username=savor;Password=savor";
        var builder = new NpgsqlConnectionStringBuilder(configuredConnectionString);
        _adminConnectionString = builder.ConnectionString;
        _databaseName = $"savor_integration_{Guid.NewGuid():N}";
        builder.Database = _databaseName;
        ConnectionString = builder.ConnectionString;

        CreateDatabase();
        Factory = new IntegrationWebApplicationFactory(ConnectionString);
        using var scope = Factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<SavorDbContext>()
            .Database.Migrate();
    }

    public string ConnectionString { get; }

    public WebApplicationFactory<Program> Factory { get; } = null!;

    public HttpClient CreateClient() =>
        Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

    public void Dispose()
    {
        Factory.Dispose();
        using var connection = new NpgsqlConnection(_adminConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""DROP DATABASE IF EXISTS "{_databaseName}" WITH (FORCE);""";
        command.ExecuteNonQuery();
    }

    private void CreateDatabase()
    {
        using var connection = new NpgsqlConnection(_adminConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"""CREATE DATABASE "{_databaseName}";""";
        command.ExecuteNonQuery();
    }

    private sealed class IntegrationWebApplicationFactory(string connectionString)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Database", connectionString);
        }
    }
}
