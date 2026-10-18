using System.ComponentModel.DataAnnotations;
using Savor.Contracts.Recipes;
using Xunit;

namespace Savor.Contracts.Tests;

public sealed class RecipeRequestValidationTests
{
    [Fact]
    public void CreateRecipeRequest_RequiresTitleAndDescription()
    {
        var request = new CreateRecipeRequest
        {
            Title = string.Empty,
            Description = string.Empty
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Title)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Description)));
    }

    [Fact]
    public void UpdateRecipeRequest_RejectsValuesLongerThanMaximumLengths()
    {
        var request = new UpdateRecipeRequest
        {
            Title = new string('t', 201),
            Description = new string('d', 4001)
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Title)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Description)));
    }

    [Fact]
    public void RecipeIngredientRequest_RejectsInvalidQuantity()
    {
        var request = new RecipeIngredientRequest
        {
            Name = "salt",
            Quantity = 0,
            Unit = "tsp"
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Quantity)));
    }

    [Fact]
    public void RecipeIngredientRequest_RejectsMissingNameAndUnit()
    {
        var request = new RecipeIngredientRequest
        {
            Name = string.Empty,
            Quantity = 1,
            Unit = string.Empty
        };

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Name)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.Unit)));
    }

    [Fact]
    public void ValidRecipeRequestsHaveNoValidationErrors()
    {
        var recipe = new CreateRecipeRequest
        {
            Title = "Tomato soup",
            Description = "A simple soup.",
            Ingredients =
            [
                new RecipeIngredientRequest
                {
                    Name = "tomato",
                    Quantity = 400,
                    Unit = "g"
                }
            ],
            Tags = ["quick"]
        };

        Assert.Empty(Validate(recipe));
    }

    [Fact]
    public void RecipeStepRequest_RejectsMissingInstruction()
    {
        var step = new RecipeStepRequest { Instruction = string.Empty };

        var errors = Validate(step);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(step.Instruction)));
    }

    private static IReadOnlyList<ValidationResult> Validate(object instance)
    {
        var context = new ValidationContext(instance);
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(instance, context, errors, validateAllProperties: true);
        return errors;
    }
}
