using Godot;

namespace ProjectSilicon.Scripts.World.Resources;

public static class ResourceSpriteDatabase
{
    private static readonly Texture2D SilicaTexture =
        GD.Load<Texture2D>(
            "res://Assets/Sprites/Resources/silica.png"
        );

    private static readonly Texture2D SiliconTexture =
        GD.Load<Texture2D>(
            "res://Assets/Sprites/Resources/silicon_ingot.png"
        );

    private static readonly Texture2D SiliconCrystalTexture =
        GD.Load<Texture2D>(
            "res://Assets/Sprites/Resources/silicon_crystal.png"
        );

    public static Texture2D Get(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Silica =>
                SilicaTexture,

            ResourceType.Silicon =>
                SiliconTexture,

            ResourceType.SiliconCrystal =>
                SiliconCrystalTexture,

            _ => throw new System.ArgumentOutOfRangeException(
                nameof(resourceType),
                resourceType,
                "Unknown resource type."
            )
        };
    }
}