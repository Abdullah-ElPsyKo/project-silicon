namespace ProjectSilicon.Scripts.World.Resources;

public sealed class RecipeDefinition
{
    public ResourceType? InputResource { get; }
    public int InputAmount { get; }
    
    public ResourceType OutputResource { get; }
    public int OutputAmount { get; }
    
    public double Duration { get; }

    public RecipeDefinition(
        ResourceType? inputResource,
        int inputAmount,
        ResourceType outputResource,
        int outputAmount,
        double duration
        )
    {
        InputResource = inputResource;
        InputAmount = inputAmount;
        OutputResource = outputResource;
        OutputAmount = outputAmount;
        Duration = duration;
    }
}