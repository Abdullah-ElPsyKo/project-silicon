using Godot;

namespace ProjectSilicon.Scripts.World;

public partial class BuildLayer
{
    // Handles build selection, rotation and mouse interactions.
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
            case MouseButton.Middle:
                ShowDetailsObject(_hoveredCell);
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

    private void SelectObject(BuildObjectType objectType)
    {
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
}