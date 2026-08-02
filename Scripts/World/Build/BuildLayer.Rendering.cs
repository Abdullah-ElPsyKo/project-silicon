using System;
using Godot;
using ProjectSilicon.Scripts.World.Belts;
using ProjectSilicon.Scripts.World.Machines;
using ProjectSilicon.Scripts.World.Resources;

namespace ProjectSilicon.Scripts.World.Build;

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
			Rect2 machineRect = GetCellRectangle(cell);

			Texture2D texture =
				MachineSpriteDatabase.Get(machine.Type);

			DrawTextureRect(
				texture,
				machineRect,
				tile: false
			);
			
			DrawMachinePorts(
				cell,
				machine
			);

			DrawRect(
				machineRect,
				new Color("e5efff"),
				filled: false,
				width: 1
			);

			DrawMachineProgress(machineRect, machine);
		}
	}
	
	private void DrawMachinePorts(
		Vector2I cell,
		MachineInstance machine)
	{
		Rect2 cellRect = GetCellRectangle(cell);
		Vector2 center = cellRect.GetCenter();

		MachineDefinition definition =
			MachineDatabase.Get(machine.Type);

		RecipeDefinition recipe =
			definition.Recipe;

		Vector2 outputDirection =
			GetDirectionVector(machine.Direction);

		DrawPortMarker(
			center,
			outputDirection,
			new Color(1.0f, 0.65f, 0.1f)
		);

		if (recipe.InputResource is null)
		{
			return;
		}

		Vector2 inputDirection =
			-outputDirection;

		DrawPortMarker(
			center,
			inputDirection,
			new Color(0.2f, 0.75f, 1.0f)
		);
	}
	
	private static Vector2 GetDirectionVector(
		Direction direction)
	{
		return direction switch
		{
			Direction.Up => Vector2.Up,
			Direction.Right => Vector2.Right,
			Direction.Down => Vector2.Down,
			Direction.Left => Vector2.Left,
			_ => Vector2.Zero
		};
	}
	
	private void DrawPortMarker(
		Vector2 center,
		Vector2 direction,
		Color color)
	{
		const float distanceFromCenter = 17.0f;
		const float markerRadius = 4.0f;

		Vector2 markerPosition =
			center + direction * distanceFromCenter;

		// Dark outline so the marker stays visible on every sprite.
		DrawCircle(
			markerPosition,
			markerRadius + 2.0f,
			new Color(0.03f, 0.03f, 0.03f, 0.95f)
		);

		DrawCircle(
			markerPosition,
			markerRadius,
			color
		);

		// Small direction notch.
		Vector2 perpendicular = new(
			-direction.Y,
			direction.X
		);

		Vector2 tip =
			markerPosition + direction * 3.0f;

		Vector2 left =
			markerPosition -
			direction * 2.0f +
			perpendicular * 2.0f;

		Vector2 right =
			markerPosition -
			direction * 2.0f -
			perpendicular * 2.0f;

		DrawColoredPolygon(
			new[]
			{
				tip,
				left,
				right
			},
			Colors.White
		);
	}

	private void DrawBelts()
	{
		foreach ((Vector2I cell, BeltInstance belt) in _belts)
		{
			Direction? inputDirection =
				FindBeltInputDirection(cell);

			bool isCorner =
				inputDirection is Direction incoming &&
				!AreDirectionsOpposite(
					incoming,
					belt.BeltDirection
				);

			Texture2D texture = isCorner
				? BeltSpriteDatabase.Corner
				: BeltSpriteDatabase.Straight;

			float rotation = isCorner
				? GetCornerRotation(
					inputDirection!.Value,
					belt.BeltDirection
				)
				: GetStraightBeltRotation(
					belt.BeltDirection
				);

			DrawBeltTexture(
				cell,
				texture,
				rotation
			);

			DrawBeltItem(cell, belt);
		}
	}
	
	private Direction? FindBeltInputDirection(Vector2I cell)
	{
		foreach (Direction direction in Enum.GetValues<Direction>())
		{
			Vector2I neighbourCell =
				cell + GetDirectionOffset(direction);

			if (!_belts.TryGetValue(
				    neighbourCell,
				    out BeltInstance? neighbourBelt))
			{
				continue;
			}

			Vector2I neighbourOutputCell =
				neighbourCell +
				GetDirectionOffset(neighbourBelt.BeltDirection);

			if (neighbourOutputCell == cell)
			{
				return direction;
			}
		}

		return null;
	}
	
	private void DrawBeltTexture(
		Vector2I cell,
		Texture2D texture,
		float rotation)
	{
		Rect2 cellRect = GetCellRectangle(cell);
		Vector2 center = cellRect.GetCenter();

		// Temporarily move the drawing origin to the tile centre,
		// then rotate everything drawn afterward.
		DrawSetTransform(
			center,
			rotation,
			Vector2.One
		);

		Rect2 localRect = new(
			-TileSize / 2.0f,
			-TileSize / 2.0f,
			TileSize,
			TileSize
		);

		DrawTextureRect(
			texture,
			localRect,
			tile: false
		);

		// Reset the canvas transform so later objects are drawn normally.
		DrawSetTransform(
			Vector2.Zero,
			0.0f,
			Vector2.One
		);
	}
	
	private static float GetCornerRotation(
		Direction inputDirection,
		Direction outputDirection)
	{
		return (inputDirection, outputDirection) switch
		{
			// Input neighbour is below, output goes right.
			(Direction.Down, Direction.Right) =>
				0.0f,

			// Input neighbour is left, output goes down.
			(Direction.Left, Direction.Down) =>
				Mathf.Pi / 2.0f,

			// Input neighbour is above, output goes left.
			(Direction.Up, Direction.Left) =>
				Mathf.Pi,

			// Input neighbour is right, output goes up.
			(Direction.Right, Direction.Up) =>
				-Mathf.Pi / 2.0f,

			// Reverse-flow versions of the same physical corners.
			(Direction.Right, Direction.Down) =>
				0.0f,

			(Direction.Down, Direction.Left) =>
				Mathf.Pi / 2.0f,

			(Direction.Left, Direction.Up) =>
				Mathf.Pi,

			(Direction.Up, Direction.Right) =>
				-Mathf.Pi / 2.0f,

			_ => 0.0f
		};
	}
	
	private static bool AreDirectionsOpposite(
		Direction first,
		Direction second)
	{
		return
			(first == Direction.Up &&
			 second == Direction.Down) ||

			(first == Direction.Down &&
			 second == Direction.Up) ||

			(first == Direction.Left &&
			 second == Direction.Right) ||

			(first == Direction.Right &&
			 second == Direction.Left);
	}
	
	private static float GetStraightBeltRotation(
		Direction direction)
	{
		return direction switch
		{
			Direction.Right => 0.0f,
			Direction.Left => Mathf.Pi,
			Direction.Down => Mathf.Pi / 2.0f,
			Direction.Up => -Mathf.Pi / 2.0f,
			_ => 0.0f
		};
	}
	
	private static Vector2I GetDirectionOffset(Direction direction)
	{
		return direction switch
		{
			Direction.Up => Vector2I.Up,
			Direction.Right => Vector2I.Right,
			Direction.Down => Vector2I.Down,
			Direction.Left => Vector2I.Left,
			_ => Vector2I.Zero
		};
	}

	private void DrawBeltItem(
		Vector2I cell,
		BeltInstance belt)
	{
		if (belt.CurrentItem is not ResourceType resourceType)
		{
			return;
		}

		Rect2 beltRect =
			GetCellRectangle(cell).Grow(-4);

		Vector2 direction =
			belt.BeltDirection switch
			{
				Direction.Up => Vector2.Up,
				Direction.Right => Vector2.Right,
				Direction.Down => Vector2.Down,
				Direction.Left => Vector2.Left,
				_ => Vector2.Zero
			};

		float progress =
			(float)belt.ItemProgress;

		Vector2 start =
			beltRect.GetCenter() -
			direction * 10;

		Vector2 end =
			beltRect.GetCenter() +
			direction * 10;

		Vector2 itemPosition =
			start.Lerp(end, progress);

		Texture2D texture =
			ResourceSpriteDatabase.Get(resourceType);

		const float itemSize = 14.0f;

		Rect2 itemRect = new(
			itemPosition.X - itemSize / 2.0f,
			itemPosition.Y - itemSize / 2.0f,
			itemSize,
			itemSize
		);

		DrawTextureRect(
			texture,
			itemRect,
			tile: false
		);
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