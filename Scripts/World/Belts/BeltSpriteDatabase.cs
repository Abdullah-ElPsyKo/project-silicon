using Godot;

namespace ProjectSilicon.Scripts.World.Belts;

public static class BeltSpriteDatabase
{
    private static readonly Texture2D StraightTexture =
        GD.Load<Texture2D>(
            "res://Assets/Sprites/Belts/belt.png"
        );

    private static readonly Texture2D CornerTexture =
        GD.Load<Texture2D>(
            "res://Assets/Sprites/Belts/belt_corner.png"
        );

    public static Texture2D Straight => StraightTexture;

    public static Texture2D Corner => CornerTexture;
}