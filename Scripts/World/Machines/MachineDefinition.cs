using Godot;
using ProjectSilicon.Scripts.World.Resources;

namespace ProjectSilicon.Scripts.World.Machines;

public sealed class MachineDefinition
{
	public string Name { get; }

	public Color Color { get; }

	public string Description { get; }

	public RecipeDefinition Recipe { get; }
	
	public int MaxInput { get; }
	
	public int MaxOutput { get; }
	
	public MachineDefinition(
		string name,
		Color color,
		string description,
		RecipeDefinition recipe,
		int maxInput,
		int maxOutput
		)
	{
		Name = name;
		Color = color;
		Description = description;
		Recipe = recipe;
		MaxInput = maxInput;
		MaxOutput = maxOutput;
	}
}
