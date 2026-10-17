using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Savor.Api.Endpoints;
using Savor.Application;
using Savor.Contracts.Common;
using Savor.Infrastructure;
using Savor.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddAuthentication()
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () =>
    Results.Ok(new HealthResponse("Healthy", "Savor", DateTimeOffset.UtcNow)))
    .WithName("GetHealth");

app.MapRecipeEndpoints();
app.MapUserEndpoints();
app.MapAuthEndpoints();

app.Run();

public partial class Program;
