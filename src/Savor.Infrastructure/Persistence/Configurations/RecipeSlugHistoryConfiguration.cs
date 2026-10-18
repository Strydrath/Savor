using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savor.Domain.Recipes;

namespace Savor.Infrastructure.Persistence.Configurations;

internal sealed class RecipeSlugHistoryConfiguration : IEntityTypeConfiguration<RecipeSlugHistory>
{
    public void Configure(EntityTypeBuilder<RecipeSlugHistory> builder)
    {
        builder.ToTable("recipe_slug_history");
        builder.HasKey(history => history.Slug);
        builder.Property(history => history.Slug).HasMaxLength(220).IsRequired();
        builder.Property(history => history.CreatedAt).IsRequired();
        builder.HasOne<Recipe>()
            .WithMany()
            .HasForeignKey(history => history.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(history => history.RecipeId);
    }
}
