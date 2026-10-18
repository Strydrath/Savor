namespace Savor.Contracts.Recipes;

/// <summary>Represents one ordered instruction in a recipe.</summary>
public sealed record RecipeStepResponse(int StepNumber, string Instruction);
