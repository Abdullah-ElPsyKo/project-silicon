using Godot;

namespace ProjectSilicon.scripts.world;

public partial class GridWorld : Node2D
{
	private const int TileSize = 32;
	private const int WorldWidth = 100;
	private const int WorldHeight = 100;

	public override void _Draw()
	{
		var groundColor = new Color("24352b");
		var gridColor = new Color("344b3d");

		var worldSize = new Vector2(
			WorldWidth * TileSize,
			WorldHeight * TileSize
		);
		
		DrawRect(
			new Rect2(Vector2.Zero, worldSize), 
			groundColor
		);

		for (var x = 0; x <= WorldWidth; x++)
		{
			var start = new Vector2(x * TileSize, 0);
			var end = new Vector2(x * TileSize, worldSize.Y);
			
			DrawLine(start, end, gridColor);
		}

		for (var y = 0; y <= WorldHeight; y++)
		{
			var start = new Vector2(0, y * TileSize);
			var end = new Vector2(worldSize.X, y * TileSize);
			
			DrawLine(start, end, gridColor);
		}
	}
}
