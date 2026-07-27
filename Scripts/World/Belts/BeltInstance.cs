using System;
using ProjectSilicon.Scripts.World.Resources;

namespace ProjectSilicon.Scripts.World.Belts;

public class BeltInstance
{
    public Direction BeltDirection { get; }
    public ResourceType? CurrentItem { get; private set; }
    public double Speed { get; }
    public double ItemProgress { get; private set; }

    public bool IsEmpty => CurrentItem == null;

    public BeltInstance(Direction direction, double speed)
    {
        BeltDirection = direction;
        Speed = speed;
    }

    public bool TryPlaceItem(ResourceType item)
    {
        if (!IsEmpty)
            return false;

        CurrentItem = item;
        ItemProgress = 0.0;
        return true;
    }

    public void Advance(double delta)
    {
        if (IsEmpty)
            return;

        ItemProgress = Math.Min(
            ItemProgress + delta * Speed,
            1.0
        );
    }

    public ResourceType? TakeItem()
    {
        if (IsEmpty)
            return null;

        ResourceType item = CurrentItem.Value;

        CurrentItem = null;
        ItemProgress = 0.0;

        return item;
    }
}