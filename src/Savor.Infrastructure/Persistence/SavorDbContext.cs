using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Savor.Domain.Recipes;
using Savor.Infrastructure.Identity;

namespace Savor.Infrastructure.Persistence;

public sealed class SavorDbContext(DbContextOptions<SavorDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SavorDbContext).Assembly);

        var publicRecipeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var privateRecipeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var ownerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var chickenStirFryId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var gardenOmeletteId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var garlicBreadId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<Recipe>().HasData(
            new
            {
                Id = publicRecipeId,
                OwnerId = ownerId,
                Title = "Creamy Tomato Pasta",
                Description = "A quick pasta with tomato, garlic, and cream.",
                Visibility = RecipeVisibility.Public
            },
            new
            {
                Id = privateRecipeId,
                OwnerId = ownerId,
                Title = "Family Sunday Stew",
                Description = "A private recipe used to verify visibility filtering.",
                Visibility = RecipeVisibility.Private
            },
            new
            {
                Id = chickenStirFryId,
                OwnerId = ownerId,
                Title = "Garlic Chicken Stir-Fry",
                Description = "A fast stir-fry with chicken, garlic, and vegetables.",
                Visibility = RecipeVisibility.Public
            },
            new
            {
                Id = gardenOmeletteId,
                OwnerId = ownerId,
                Title = "Garden Omelette",
                Description = "A fluffy omelette with eggs, tomato, and onion.",
                Visibility = RecipeVisibility.Public
            },
            new
            {
                Id = garlicBreadId,
                OwnerId = ownerId,
                Title = "Garlic Bread",
                Description = "Golden bread with garlic and butter.",
                Visibility = RecipeVisibility.Public
            });

        modelBuilder.Entity<RecipeIngredient>().HasData(
            new { RecipeId = publicRecipeId, Name = "tomato", Quantity = 400m, Unit = "g" },
            new { RecipeId = publicRecipeId, Name = "pasta", Quantity = 250m, Unit = "g" },
            new { RecipeId = publicRecipeId, Name = "garlic", Quantity = 2m, Unit = "cloves" },
            new { RecipeId = privateRecipeId, Name = "beef", Quantity = 500m, Unit = "g" });
        modelBuilder.Entity<RecipeIngredient>().HasData(
            new { RecipeId = chickenStirFryId, Name = "chicken", Quantity = 300m, Unit = "g" },
            new { RecipeId = chickenStirFryId, Name = "garlic", Quantity = 2m, Unit = "cloves" },
            new { RecipeId = chickenStirFryId, Name = "onion", Quantity = 1m, Unit = "piece" },
            new { RecipeId = gardenOmeletteId, Name = "egg", Quantity = 3m, Unit = "pieces" },
            new { RecipeId = gardenOmeletteId, Name = "tomato", Quantity = 1m, Unit = "piece" },
            new { RecipeId = gardenOmeletteId, Name = "onion", Quantity = 0.5m, Unit = "piece" },
            new { RecipeId = garlicBreadId, Name = "bread", Quantity = 1m, Unit = "loaf" },
            new { RecipeId = garlicBreadId, Name = "garlic", Quantity = 3m, Unit = "cloves" },
            new { RecipeId = garlicBreadId, Name = "butter", Quantity = 50m, Unit = "g" });

        modelBuilder.Entity<RecipeTag>().HasData(
            new { RecipeId = publicRecipeId, Name = "quick" },
            new { RecipeId = publicRecipeId, Name = "pasta" },
            new { RecipeId = privateRecipeId, Name = "family" });
        modelBuilder.Entity<RecipeTag>().HasData(
            new { RecipeId = chickenStirFryId, Name = "quick" },
            new { RecipeId = gardenOmeletteId, Name = "breakfast" },
            new { RecipeId = garlicBreadId, Name = "side" });

        modelBuilder.Entity<RecipeStep>().HasData(
            new { RecipeId = publicRecipeId, StepNumber = 1, Instruction = "Cook the pasta until al dente." },
            new { RecipeId = publicRecipeId, StepNumber = 2, Instruction = "Prepare the tomato and garlic sauce." },
            new { RecipeId = publicRecipeId, StepNumber = 3, Instruction = "Toss the pasta with the sauce and serve." },
            new { RecipeId = privateRecipeId, StepNumber = 1, Instruction = "Simmer the stew until tender." });
        modelBuilder.Entity<RecipeStep>().HasData(
            new { RecipeId = chickenStirFryId, StepNumber = 1, Instruction = "Stir-fry the chicken until cooked." },
            new { RecipeId = chickenStirFryId, StepNumber = 2, Instruction = "Add the garlic and onion, then serve." },
            new { RecipeId = gardenOmeletteId, StepNumber = 1, Instruction = "Whisk the eggs and cook with the vegetables." },
            new { RecipeId = garlicBreadId, StepNumber = 1, Instruction = "Spread garlic butter on the bread and bake." });
    }
}
