using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savor.Domain.Recipes;

namespace Savor.Infrastructure.Persistence.Configurations;

internal sealed class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable("recipe_ingredients");
        builder.HasKey(ingredient => new { ingredient.RecipeId, ingredient.Name });
        builder.Property(ingredient => ingredient.Name).HasMaxLength(100).IsRequired();
        builder.Property(ingredient => ingredient.Quantity).HasPrecision(10, 2).IsRequired();
        builder.Property(ingredient => ingredient.Unit).HasMaxLength(30).IsRequired();
    }
}
