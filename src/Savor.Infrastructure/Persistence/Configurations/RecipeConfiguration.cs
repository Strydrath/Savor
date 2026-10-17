using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savor.Domain.Recipes;

namespace Savor.Infrastructure.Persistence.Configurations;

internal sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("recipes");
        builder.HasKey(recipe => recipe.Id);
        builder.Property(recipe => recipe.Title).HasMaxLength(200).IsRequired();
        builder.Property(recipe => recipe.Description).HasMaxLength(4000).IsRequired();
        builder.Property(recipe => recipe.Visibility).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.HasMany(recipe => recipe.Ingredients)
            .WithOne()
            .HasForeignKey(ingredient => ingredient.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(recipe => recipe.Tags)
            .WithOne()
            .HasForeignKey(tag => tag.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
