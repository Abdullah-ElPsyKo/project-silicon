using Godot;

namespace ProjectSilicon.Scripts.World;

public partial class BuildLayer
{
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