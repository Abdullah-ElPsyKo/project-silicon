namespace ProjectSilicon.Scripts.World;

public sealed class RecipeDefinition
{
    public ResourceType? InputResource { get; }
    public int InputAmount { get; }
    
    public ResourceType OutputResource { get; }
    public int OutputAmount { get; }

    public RecipeDefinition(
        ResourceType? inputResource,
        int inputAmount,
        ResourceType outputResource,
        int outputAmount)
    {
        InputResource = inputResource;
        InputAmount = inputAmount;
        OutputResource = outputResource;
        OutputAmount = outputAmount;
    }
}