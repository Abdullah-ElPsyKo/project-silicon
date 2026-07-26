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
					SelectObject(BuildObjectType.SilicaExtractor);
					break;

				case Key.Key2:
					SelectObject(BuildObjectType.SiliconSmelter);
					break;

				case Key.Key3:
					SelectObject(BuildObjectType.CrystalGrower);
					break;

				case Key.Key4:
					SelectObject(BuildObjectType.Belt);
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
				PlaceObject(_hoveredCell);
				break;

			case MouseButton.Right:
				RemoveObject(_hoveredCell);
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

	private void SelectObject(BuildObjectType objectType)	{
		if (_selectedObject == objectType)
			return;

		_selectedObject = objectType;

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
	
	// PLACE OBJECT LOGIC

	private void PlaceObject(Vector2I cell)
	{
		bool occupied =
			_machines.ContainsKey(cell) ||
			_belts.ContainsKey(cell);

		if (occupied)
			return;

		switch (_selectedObject)
		{
			case BuildObjectType.SilicaExtractor:
				PlaceMachine(cell, MachineType.SilicaExtractor);
				break;

			case BuildObjectType.SiliconSmelter:
				PlaceMachine(cell, MachineType.SiliconSmelter);
				break;

			case BuildObjectType.CrystalGrower:
				PlaceMachine(cell, MachineType.CrystalGrower);
				break;

			case BuildObjectType.Belt:
				PlaceBelt(cell);
				break;
		}
	}
	
	private void PlaceMachine(
		Vector2I cell,
		MachineType objectType)
	{
		MachineInstance machine = new(
			objectType,
			_currentDirection
		);

		_machines.Add(cell, machine);
	}
	
	private void PlaceBelt(Vector2I cell)
	{
		BeltInstance belt = new(
			_currentDirection,
			1.0
		);

		_belts.Add(cell, belt);
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
			[4] Belt
			[R] Rotate

			Selected: {GetSelectedObjectName()}
			Rotation: {_currentDirection}
			""";
	}
	
	private string GetSelectedObjectName()
	{
		return _selectedObject switch
		{
			BuildObjectType.SilicaExtractor =>
				MachineDatabase.Get(MachineType.SilicaExtractor).Name,

			BuildObjectType.SiliconSmelter =>
				MachineDatabase.Get(MachineType.SiliconSmelter).Name,

			BuildObjectType.CrystalGrower =>
				MachineDatabase.Get(MachineType.CrystalGrower).Name,

			BuildObjectType.Belt =>
				"Belt",

			_ => "Unknown"
		};
	}

	public override void _Draw()
	{
		DrawMachines();
		DrawBelts();
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
	
	private void DrawBelts()
	{
		foreach ((Vector2I cell, BeltInstance belt) in _belts)
		{
			Rect2 beltRect = GetCellRectangle(cell).Grow(-4);

			DrawRect(
				beltRect,
				new Color(0.18f, 0.18f, 0.18f)
			);

			Vector2 center = beltRect.GetCenter();

			Vector2 direction = belt.BeltDirection switch
			{
				Direction.Up => Vector2.Up,
				Direction.Right => Vector2.Right,
				Direction.Down => Vector2.Down,
				Direction.Left => Vector2.Left,
				_ => Vector2.Zero
			};

			Vector2 start = center - direction * 7;
			Vector2 end = center + direction * 7;

			DrawLine(
				start,
				end,
				new Color(0.85f, 0.85f, 0.85f),
				2
			);

			Vector2 perpendicular = new(
				-direction.Y,
				direction.X
			);

			Vector2 arrowLeft =
				end - direction * 4 + perpendicular * 3;

			Vector2 arrowRight =
				end - direction * 4 - perpendicular * 3;

			DrawLine(end, arrowLeft, new Color(0.85f, 0.85f, 0.85f), 2);
			DrawLine(end, arrowRight, new Color(0.85f, 0.85f, 0.85f), 2);
			
			DrawBeltItem(cell, belt);
		}
	}
	
	private void DrawBeltItem(Vector2I cell, BeltInstance belt)
	{
		if (belt.IsEmpty)
		{
			return;
		}

		Rect2 beltRect = GetCellRectangle(cell).Grow(-4);

		Vector2 direction = belt.BeltDirection switch
		{
			Direction.Up => Vector2.Up,
			Direction.Right => Vector2.Right,
			Direction.Down => Vector2.Down,
			Direction.Left => Vector2.Left,
			_ => Vector2.Zero
		};

		float progress = (float)belt.ItemProgress;

		Vector2 start = beltRect.GetCenter() - direction * 10;
		Vector2 end = beltRect.GetCenter() + direction * 10;
		Vector2 itemPosition = start.Lerp(end, progress);
		
		DrawCircle(itemPosition, 4, new Color(0.9f, 0.8f, 0.3f));
	}

	private void RemoveObject(Vector2I cell)
	{
		bool removedMachine = _machines.Remove(cell);
		bool removedBelt = _belts.Remove(cell);

		if (removedMachine || removedBelt)
			QueueRedraw();
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
			Color objectColor;

			if (_selectedObject == BuildObjectType.Belt)
			{
				objectColor = new Color(
					0.3f,
					0.3f,
					0.3f
				);
			}
			else
			{
				MachineType machineType = _selectedObject switch
				{
					BuildObjectType.SilicaExtractor =>
						MachineType.SilicaExtractor,

					BuildObjectType.SiliconSmelter =>
						MachineType.SiliconSmelter,

					BuildObjectType.CrystalGrower =>
						MachineType.CrystalGrower,

					_ => MachineType.SilicaExtractor
				};

				objectColor = MachineDatabase.Get(machineType).Color;
			}

			hoverColor = new Color(
				objectColor.R,
				objectColor.G,
				objectColor.B,
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
