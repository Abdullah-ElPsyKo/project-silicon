using System.Collections.Generic;
using Godot;

namespace ProjectSilicon.Scripts.World;

public class BeltSystem
{
    public static void Update(
        Dictionary<Vector2I, BeltInstance> belts,
        double delta)
    {
        List<(BeltInstance Source, BeltInstance Destination)> transfers = new();
		
        // Phase 1: Advance all belts.
        foreach (BeltInstance belt in belts.Values)
        {
            belt.Advance(delta);
        }
		
        // Phase 2: Look for all possible transfers.
        foreach (KeyValuePair<Vector2I, BeltInstance> entry in belts)
        {
            Vector2I position = entry.Key;
            BeltInstance sourceBelt = entry.Value;

            if (sourceBelt.CurrentItem == null)
                continue;

            if (sourceBelt.ItemProgress < 1.0)
                continue;

            Vector2I directionOffset =
                GetDirectionOffset(sourceBelt.BeltDirection);

            Vector2I destinationPosition =
                position + directionOffset;

            if (!belts.TryGetValue(
                    destinationPosition,
                    out BeltInstance? destinationBelt))
            {
                continue;
            }

            if (!destinationBelt.IsEmpty)
                continue;

            transfers.Add((sourceBelt, destinationBelt));
        }
		
        // Phase 3: Execute transfers.
        foreach ((BeltInstance source, BeltInstance destination) in transfers)
        {
            ResourceType item = source.CurrentItem.Value;

            if (!destination.TryPlaceItem(item))
                continue;

            source.TakeItem();
        }
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
}