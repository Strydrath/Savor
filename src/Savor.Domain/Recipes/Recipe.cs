using Savor.Domain.Common;

namespace Savor.Domain.Recipes;

public sealed class Recipe : Entity
{
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<RecipeTag> _tags = [];
    private readonly List<RecipeStep> _steps = [];

    private Recipe()
    {
    }

    public Recipe(Guid ownerId, string title, string description, RecipeVisibility visibility)
    {
        OwnerId = ownerId;
        Title = title;
        Description = description;
        Visibility = visibility;
    }

    public Guid OwnerId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public RecipeVisibility Visibility { get; private set; }
    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients;
    public IReadOnlyCollection<RecipeTag> Tags => _tags;
    public IReadOnlyCollection<RecipeStep> Steps => _steps;

    public void AddIngredient(string name, decimal quantity, string unit)
    {
        _ingredients.Add(new RecipeIngredient(Id, name, quantity, unit));
    }

    public void AddTag(string name)
    {
        _tags.Add(new RecipeTag(Id, name));
    }

    public void AddStep(int stepNumber, string instruction)
    {
        _steps.Add(new RecipeStep(Id, stepNumber, instruction));
    }

    public void Update(
        string title,
        string description,
        RecipeVisibility visibility)
    {
        Title = title;
        Description = description;
        Visibility = visibility;
        _ingredients.Clear();
        _tags.Clear();
        _steps.Clear();
    }
}
