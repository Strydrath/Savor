namespace Savor.Domain.Recipes;

public sealed class RecipeStep
{
    private RecipeStep()
    {
    }

    internal RecipeStep(Guid recipeId, int stepNumber, string instruction)
    {
        RecipeId = recipeId;
        StepNumber = stepNumber;
        Instruction = instruction;
    }

    public Guid RecipeId { get; private set; }
    public int StepNumber { get; private set; }
    public string Instruction { get; private set; } = string.Empty;
}
