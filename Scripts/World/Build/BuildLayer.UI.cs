using ProjectSilicon.Scripts.World.Machines;

namespace ProjectSilicon.Scripts.World.Build;


public partial class BuildLayer
{
    // Updates the small build-selection HUD.
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
}