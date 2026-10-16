using Microsoft.EntityFrameworkCore;
using Savor.Domain.Recipes;

namespace Savor.Infrastructure.Persistence;

public sealed class SavorDbContext(DbContextOptions<SavorDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    public DbSet<RecipeTag> RecipeTags => Set<RecipeTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SavorDbContext).Assembly);

        var publicRecipeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var privateRecipeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var ownerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

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
            });

        modelBuilder.Entity<RecipeIngredient>().HasData(
            new { RecipeId = publicRecipeId, Name = "tomato", Quantity = 400m, Unit = "g" },
            new { RecipeId = publicRecipeId, Name = "pasta", Quantity = 250m, Unit = "g" },
            new { RecipeId = publicRecipeId, Name = "garlic", Quantity = 2m, Unit = "cloves" },
            new { RecipeId = privateRecipeId, Name = "beef", Quantity = 500m, Unit = "g" });

        modelBuilder.Entity<RecipeTag>().HasData(
            new { RecipeId = publicRecipeId, Name = "quick" },
            new { RecipeId = publicRecipeId, Name = "pasta" },
            new { RecipeId = privateRecipeId, Name = "family" });
    }
}
