using System.Collections.Generic;
using Godot;
using ProjectSilicon.Scripts.World.Belts;
using ProjectSilicon.Scripts.World.Machines;

namespace ProjectSilicon.Scripts.World.Build;

public partial class BuildLayer : Node2D
{
	private const int TileSize = 32;
	
	[Export] public Label? ResourceLabel { get; set; }
	[Export] public Label? ObjectDetailsLabel { get; set; }

	// World state.
	private readonly Dictionary<Vector2I, MachineInstance> _machines = new();
	private readonly Dictionary<Vector2I, BeltInstance> _belts = new();

	// Current interaction state.
	private Vector2I _hoveredCell;
	private Vector2I? _inspectedCell;
	private BuildObjectType _selectedObject = BuildObjectType.SilicaExtractor;
	private Direction _currentDirection = Direction.Down;


	public override void _Ready()
	{
		UpdateResourceLabel();
	}

	public override void _Process(double delta)
	{
		UpdateHoveredCell();

		ProductionSystem.Update(_machines, delta);
		BeltSystem.Update(_belts, delta, _machines);

		UpdateObjectDetailsLabel();
		QueueRedraw();
	}
}
