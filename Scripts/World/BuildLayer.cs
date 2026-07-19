using Godot;
using System.Collections.Generic;

namespace ProjectSilicon.Scripts.World;

public partial class BuildLayer : Node2D
{
	private const int TileSize = 32;
	private const double ProductionInterval = 1.0;

	[Export]
	public Label? ResourceLabel { get; set; }

	private readonly Dictionary<Vector2I, MachineType> _machines = new();

	private Vector2I _hoveredCell;
	private MachineType _selectedMachine = MachineType.SilicaExtractor;

	private double _productionTimer;

	private int _silica;
	private int _silicon;
	private int _siliconCrystal;

	public override void _Ready()
	{
		UpdateResourceLabel();
	}

	public override void _Process(double delta)
	{
		UpdateHoveredCell();
		RunProduction(delta);
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
		if (_machines.Count == 0)
		{
			_productionTimer = 0.0;
			return;
		}

		_productionTimer += delta;

		while (_productionTimer >= ProductionInterval)
		{
			_productionTimer -= ProductionInterval;
			RunProductionCycle();
		}
	}

	private void RunProductionCycle()
	{
		const int smelterSilicaCost = 2;
		const int crystalGrowerSiliconCost = 3;

		foreach (MachineType machineType in _machines.Values)
		{
			if (machineType == MachineType.SilicaExtractor)
			{
				_silica++;
			}
		}

		foreach (MachineType machineType in _machines.Values)
		{
			if (machineType != MachineType.SiliconSmelter)
			{
				continue;
			}

			if (_silica < smelterSilicaCost)
			{
				continue;
			}

			_silica -= smelterSilicaCost;
			_silicon++;
		}

		foreach (MachineType machineType in _machines.Values)
		{
			if (machineType != MachineType.CrystalGrower)
			{
				continue;
			}

			if (_silicon < crystalGrowerSiliconCost)
			{
				continue;
			}

			_silicon -= crystalGrowerSiliconCost;
			_siliconCrystal++;
		}

		UpdateResourceLabel();
	}

	private void PlaceMachine(Vector2I cell)
	{
		if (_machines.TryAdd(cell, _selectedMachine))
		{
			QueueRedraw();
		}
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
			Silica: {_silica}
			Silicon: {_silicon}
			Silicon Crystal: {_siliconCrystal}

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
		foreach ((Vector2I cell, MachineType machineType) in _machines)
		{
			Rect2 machineRect = GetCellRectangle(cell).Grow(-3);
			MachineDefinition definition = MachineDatabase.Get(machineType);

			DrawRect(machineRect, definition.Color);

			DrawRect(
				machineRect,
				new Color("e5efff"),
				filled: false,
				width: 2
			);
		}
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