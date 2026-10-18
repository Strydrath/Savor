using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Savor.Domain.Recipes;

namespace Savor.Infrastructure.Persistence.Configurations;

internal sealed class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.ToTable("recipe_steps");
        builder.HasKey(step => new { step.RecipeId, step.StepNumber });
        builder.Property(step => step.Instruction).HasMaxLength(2000).IsRequired();
    }
}
