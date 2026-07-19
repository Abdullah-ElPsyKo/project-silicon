using Godot;

namespace ProjectSilicon.Scripts.World;

public sealed class MachineDefinition
{
	public string Name { get; }

	public Color Color { get; }

	public string Description { get; }

	public MachineDefinition(
		string name,
		Color color,
		string description)
	{
		Name = name;
		Color = color;
		Description = description;
	}
}
