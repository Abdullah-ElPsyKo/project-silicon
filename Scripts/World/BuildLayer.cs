using System.Collections.Generic;
using Godot;


namespace ProjectSilicon.Scripts.World;

public partial class BuildLayer : Node2D
{
	private const int TileSize = 32;
	private const double ProductionInterval = 1.0;

	[Export]
	public Label? ResourceLabel { get; set; }

	private readonly Dictionary<Vector2I, MachineInstance> _machines = new();
	private readonly Dictionary<ResourceType, int> _resources = new()
	{
		[ResourceType.Silica] = 0,
		[ResourceType.Silicon] = 0,
		[ResourceType.SiliconCrystal] = 0
	};
	
	private static readonly MachineType[] ProductionOrder =
	{
		MachineType.SilicaExtractor,
		MachineType.SiliconSmelter,
		MachineType.CrystalGrower
	};

	private Vector2I _hoveredCell;
	private MachineType _selectedMachine = MachineType.SilicaExtractor;

	private double _productionTimer;
	

	public override void _Ready()
	{
		UpdateResourceLabel();
	}

	public override void _Process(double delta)
	{
		UpdateHoveredCell();
		RunProduction(delta);
		QueueRedraw();
	}

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

	private void RunProduction(double delta)
	{
		foreach (MachineType machineType in ProductionOrder)
		{
			foreach (MachineInstance machine in _machines.Values)
			{
				if (machine.Type != machineType)
				{
					continue;
				}

				UpdateMachineProduction(machine, delta);
			}
		}

		UpdateResourceLabel();
	}
	
	private void UpdateMachineProduction(
		MachineInstance machine,
		double delta)
	{
		MachineDefinition definition = MachineDatabase.Get(machine.Type);
		RecipeDefinition recipe = definition.Recipe;

		if (!machine.IsProducing)
		{
			if (!TryStartProduction(machine, recipe))
			{
				return;
			}
		}

		machine.ProductionProgress += delta;

		if (machine.ProductionProgress < recipe.Duration)
		{
			return;
		}

		machine.AddOutput(
			recipe.OutputResource,
			recipe.OutputAmount
		);
		
		machine.ProductionProgress = 0.0;
		machine.IsProducing = false;
	}

	private bool TryStartProduction(MachineInstance machine, RecipeDefinition recipe)
	{
		if (recipe.InputResource is ResourceType inputResource)
		{
			if (!machine.TryConsumeInput(inputResource, recipe.InputAmount))
			{
				return false;
			}
		}

		machine.IsProducing = true;
		return true;
	}

	private void PlaceMachine(Vector2I cell)
	{
		if (_machines.ContainsKey(cell))
		{
			return;
		}
		
		MachineInstance machine = new(_selectedMachine);

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

	private void UpdateResourceLabel()
	{
		if (ResourceLabel is null)
		{
			return;
		}

		ResourceLabel.Text = $"""
			Silica: {_resources[ResourceType.Silica]}
			Silicon: {_resources[ResourceType.Silicon]}
			Silicon Crystal: {_resources[ResourceType.SiliconCrystal]}

			[1] Silica Extractor
			[2] Silicon Smelter
			[3] Crystal Grower

			Selected: {MachineDatabase.Get(_selectedMachine).Name}
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
			// GD.Print(machine.IsProducing);
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
		bool occupied = _machines.ContainsKey(_hoveredCell);

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
