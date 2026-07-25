using System.Collections.Generic;
using Godot;


namespace ProjectSilicon.Scripts.World;

public partial class BuildLayer : Node2D
{
	private const int TileSize = 32;

	[Export]
	public Label? ResourceLabel { get; set; }

	private readonly Dictionary<Vector2I, MachineInstance> _machines = new();
	private readonly Dictionary<Vector2I, BeltInstance> _belts = new();

	private Vector2I _hoveredCell;
	private MachineType _selectedMachine = MachineType.SilicaExtractor;
	
	private Direction _currentDirection = Direction.Down;
	

	public override void _Ready()
	{
		UpdateResourceLabel();
	}

	public override void _Process(double delta)
	{
		UpdateHoveredCell();

		ProductionSystem.Update(_machines, delta);
		BeltSystem.Update(_belts, delta);
		QueueRedraw();
	}


	// INPUT LOGIC
	
	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey keyEvent && keyEvent.Pressed)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Key1:
					SelectMachine(MachineType.SilicaExtractor);
					break;

				case Key.Key2:
					SelectMachine(MachineType.SiliconSmelter);
					break;

				case Key.Key3:
					SelectMachine(MachineType.CrystalGrower);
					break;
				case Key.R:
					RotateObject();
					break;
			}

			return;
		}

		if (inputEvent is not InputEventMouseButton mouseButton ||
			!mouseButton.Pressed)
		{
			return;
		}

		switch (mouseButton.ButtonIndex)
		{
			case MouseButton.Left:
				PlaceMachine(_hoveredCell);
				break;

			case MouseButton.Right:
				RemoveMachine(_hoveredCell);
				break;
		}
	}

	private void RotateObject()
	{
		if (_currentDirection == Direction.Left)
		{
			_currentDirection = Direction.Up;
		}
		else
		{
			_currentDirection++;
		}

		UpdateResourceLabel();
		QueueRedraw();
	}

	private void SelectMachine(MachineType machineType)
	{
		if (_selectedMachine == machineType)
		{
			return;
		}

		_selectedMachine = machineType;

		UpdateResourceLabel();
		QueueRedraw();
	}

	private void UpdateHoveredCell()
	{
		Vector2 mousePosition = GetGlobalMousePosition();

		Vector2I newHoveredCell = new(
			Mathf.FloorToInt(mousePosition.X / TileSize),
			Mathf.FloorToInt(mousePosition.Y / TileSize)
		);

		if (newHoveredCell == _hoveredCell)
		{
			return;
		}

		_hoveredCell = newHoveredCell;
		QueueRedraw();
	}
	
	// PLACE MACHINE LOGIC

	private void PlaceMachine(Vector2I cell)
	{
		bool occupied =
			_machines.ContainsKey(cell) ||
			_belts.ContainsKey(cell);
		
		if (occupied)
		{
			return;
		}
		
		MachineInstance machine = new(_selectedMachine, _currentDirection);

		_machines.Add(cell, machine);

		QueueRedraw();
	}

	private void RemoveMachine(Vector2I cell)
	{
		if (_machines.Remove(cell))
		{
			QueueRedraw();
		}
	}
	
	// UPDATE VISUALS

	private void UpdateResourceLabel()
	{
		if (ResourceLabel is null)
			return;

		ResourceLabel.Text = $"""
			[1] Silica Extractor
			[2] Silicon Smelter
			[3] Crystal Grower
			[R] Rotate

			Selected: {MachineDatabase.Get(_selectedMachine).Name}
			Rotation: {_currentDirection}
			""";
	}

	public override void _Draw()
	{
		DrawMachines();
		DrawHoveredCell();
	}

	private void DrawMachines()
	{
		foreach ((Vector2I cell, MachineInstance machine) in _machines)
		{
			Rect2 machineRect = GetCellRectangle(cell).Grow(-3);
			MachineDefinition definition = MachineDatabase.Get(machine.Type);
			DrawRect(machineRect, definition.Color);

			DrawRect(
				machineRect,
				new Color("e5efff"),
				filled: false,
				width: 2
			);
			DrawMachineProgress(machineRect, machine);
		}
	}

	private void DrawMachineProgress(
		Rect2 machineRect,
		MachineInstance machine)
	{
		if (!machine.IsProducing)
		{
			return;
		}

		MachineDefinition definition = MachineDatabase.Get(machine.Type);
		RecipeDefinition recipe = definition.Recipe;
		
		float progress = (float)(
			machine.ProductionProgress / recipe.Duration
		);
		
		progress = Mathf.Clamp(progress, 0.0f, 1.0f);
		
		Rect2 backgroundBar = new(
			machineRect.Position.X + 2,
			machineRect.End.Y - 6,
			machineRect.Size.X - 4,
			4
		);
		
		Rect2 filledBar = new(
			backgroundBar.Position,
			new Vector2(
				backgroundBar.Size.X * progress,
				backgroundBar.Size.Y
			)
		);
		
		DrawRect(backgroundBar, new Color(0.1f, 0.1f, 0.1f, 0.8f));
		DrawRect(filledBar, new Color(0.9f, 0.9f, 0.9f));
	}

	private void DrawHoveredCell()
	{
		bool occupied =
			_machines.ContainsKey(_hoveredCell) ||
			_belts.ContainsKey(_hoveredCell);
		
		Color hoverColor;

		if (occupied)
		{
			hoverColor = new Color(
				1.0f,
				0.25f,
				0.25f,
				0.35f
			);
		}
		else
		{
			Color machineColor = MachineDatabase.Get(_selectedMachine).Color;

			hoverColor = new Color(
				machineColor.R,
				machineColor.G,
				machineColor.B,
				0.45f
			);
		}

		DrawRect(
			GetCellRectangle(_hoveredCell),
			hoverColor
		);
	}

	private static Rect2 GetCellRectangle(Vector2I cell)
	{
		return new Rect2(
			cell.X * TileSize,
			cell.Y * TileSize,
			TileSize,
			TileSize
		);
	}
}
