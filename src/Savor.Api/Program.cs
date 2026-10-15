using Savor.Application;
using Savor.Contracts.Common;
using Savor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () =>
    Results.Ok(new HealthResponse("Healthy", "Savor", DateTimeOffset.UtcNow)))
    .WithName("GetHealth");

app.Run();

public partial class Program;
