using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savor.Domain.Recipes;

namespace Savor.Infrastructure.Persistence.Configurations;

internal sealed class RecipeTagConfiguration : IEntityTypeConfiguration<RecipeTag>
{
    public void Configure(EntityTypeBuilder<RecipeTag> builder)
    {
        builder.ToTable("recipe_tags");
        builder.HasKey(tag => new { tag.RecipeId, tag.Name });
        builder.Property(tag => tag.Name).HasMaxLength(50).IsRequired();
    }
}
