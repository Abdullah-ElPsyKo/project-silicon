using System.Collections.Generic;
using Godot;

namespace ProjectSilicon.Scripts.World;

public static class BeltSystem
{
    public static void Update(
        Dictionary<Vector2I, BeltInstance> belts,
        double delta,
        Dictionary<Vector2I, MachineInstance> machines)
    {
        List<(BeltInstance Source, BeltInstance Destination)> transfers = new();
        
        // Phase 0: Machines -> Belts
        TransferMachineOutputsToBelts(machines, belts);
		
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
        
        // Phase 4: Belts -> Machines
        TransferBeltOutputsToMachines(machines, belts);
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

    private static void TransferMachineOutputsToBelts(Dictionary<Vector2I, MachineInstance> machines,
        Dictionary<Vector2I, BeltInstance> belts)
    {
        foreach (KeyValuePair<Vector2I, MachineInstance> machineInstance in machines)
        {
            MachineInstance machine = machineInstance.Value;
            Vector2I machinePosition = machineInstance.Key;

            MachineDefinition definition = MachineDatabase.Get(machine.Type);
            RecipeDefinition recipe = definition.Recipe;
            
            Vector2I directionOffset =
                GetDirectionOffset(machine.Direction);

            Vector2I destinationPosition =
                machinePosition + directionOffset;

            if (!belts.TryGetValue(
                    destinationPosition,
                    out BeltInstance? destinationBelt)) continue;

            if (!destinationBelt.IsEmpty)
            {
                continue;
            }

            ResourceType outputResource = recipe.OutputResource;
            
            if (machine.GetOutputAmount(outputResource) < 1)
            {
                continue;
            }

            if (!destinationBelt.TryPlaceItem(outputResource))
            {
                continue;
            }
            machine.TryConsumeOutput(outputResource, 1);
        }
    }

    private static void TransferBeltOutputsToMachines(Dictionary<Vector2I, MachineInstance> machines,
        Dictionary<Vector2I, BeltInstance> belts)
    {
        foreach (KeyValuePair<Vector2I, BeltInstance> beltInstance in belts)
        {
            BeltInstance belt = beltInstance.Value;
            Vector2I beltPosition = beltInstance.Key;
            
            if (belt.CurrentItem == null)
                continue;

            if (belt.ItemProgress < 1.0)
                continue;
            
            Vector2I directionOffset =
                GetDirectionOffset(belt.BeltDirection);

            Vector2I destinationPosition =
                beltPosition + directionOffset;

            if (!machines.TryGetValue(
                    destinationPosition,
                    out MachineInstance? destinationMachine)) continue;
            
            MachineDefinition definition = MachineDatabase.Get(destinationMachine.Type);
            RecipeDefinition recipe = definition.Recipe;

            ResourceType? machineInput = recipe.InputResource;

            if (machineInput == null)
            {
                continue;
            }

            if (belt.CurrentItem != machineInput)
            {
                continue;
            }

            if (belt.CurrentItem is ResourceType item)
            {
                if (destinationMachine.TryAddInput(item, 1))
                {
                    belt.TakeItem();
                }
            }
        }
    }
}