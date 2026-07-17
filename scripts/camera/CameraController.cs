using Godot;

namespace ProjectSilicon.scripts.camera;

public partial class CameraController : Camera2D
{
	private const float MoveSpeed = 700.0f;
	private const float ZoomStep = 0.1f;
	private const float MinimumZoom = 0.5f;
	private const float MaximumZoom = 2.0f;

	public override void _Process(double delta)
	{
		var direction = Input.GetVector(
			"ui_left",
			"ui_right",
			"ui_up",
			"ui_down"
		);

		Position += direction * MoveSpeed * (float)delta / Zoom.X;
	}

	public override void _UnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent is not InputEventMouseButton mouseButton || !mouseButton.Pressed)
		{
			return;
		}
		
		var zoomAmount = mouseButton.ButtonIndex switch
		{
			MouseButton.WheelUp => ZoomStep,
			MouseButton.WheelDown => -ZoomStep,
			_ => 0.0f
		};

		if (zoomAmount == 0.0f)
		{
			return;
		}

		var newZoom = Mathf.Clamp(
			Zoom.X + zoomAmount,
			MinimumZoom,
			MaximumZoom
		);

		Zoom = new Vector2(newZoom, newZoom);
	}
}
