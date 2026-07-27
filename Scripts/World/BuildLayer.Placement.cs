using Godot;

namespace ProjectSilicon.Scripts.World;

public partial class BuildLayer
{
    // Places the currently selected build object on an empty grid cell.
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

    private void RemoveObject(Vector2I cell)
    {
        bool removedMachine = _machines.Remove(cell);
        bool removedBelt = _belts.Remove(cell);

        if (removedMachine || removedBelt)
            QueueRedraw();
    }
}