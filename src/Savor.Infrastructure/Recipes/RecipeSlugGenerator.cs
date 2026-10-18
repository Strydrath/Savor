using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Savor.Domain.Recipes;
using Savor.Infrastructure.Persistence;

namespace Savor.Infrastructure.Recipes;

internal sealed class RecipeSlugGenerator(SavorDbContext dbContext)
{
    public async Task<string> GenerateAsync(
        string title,
        Guid? excludedRecipeId,
        CancellationToken cancellationToken)
    {
        var baseSlug = ToSlug(title);
        var candidate = baseSlug;
        var suffix = 2;

        while (await dbContext.Recipes.AnyAsync(
                   recipe => recipe.Slug == candidate &&
                       (!excludedRecipeId.HasValue || recipe.Id != excludedRecipeId.Value),
                   cancellationToken)
               || await dbContext.RecipeSlugHistory.AnyAsync(
                   history => history.Slug == candidate,
                   cancellationToken))
        {
            candidate = $"{baseSlug}-{suffix++}";
        }

        return candidate;
    }

    private static string ToSlug(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : '-');
        }

        var slug = Regex.Replace(builder.ToString(), "-+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "recipe" : slug;
    }
}
