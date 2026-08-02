using Godot;

namespace ProjectSilicon.Scripts.World.Machines;

public static class MachineSpriteDatabase
{
    private static readonly Texture2D SilicaExtractorTexture =
        GD.Load<Texture2D>(
            "res://Assets/Sprites/Machines/silica_extractor.png"
        );

    private static readonly Texture2D SiliconSmelterTexture =
        GD.Load<Texture2D>(
            "res://Assets/Sprites/Machines/silicon_smelter.png"
        );

    private static readonly Texture2D CrystalGrowerTexture =
        GD.Load<Texture2D>(
            "res://Assets/Sprites/Machines/crystal_grower.png"
        );

    public static Texture2D Get(MachineType machineType)
    {
        return machineType switch
        {
            MachineType.SilicaExtractor =>
                SilicaExtractorTexture,

            MachineType.SiliconSmelter =>
                SiliconSmelterTexture,

            MachineType.CrystalGrower =>
                CrystalGrowerTexture,

            _ => throw new System.ArgumentOutOfRangeException(
                nameof(machineType),
                machineType,
                "Unknown machine type."
            )
        };
    }
}