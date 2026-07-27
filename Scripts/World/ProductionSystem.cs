using System.Collections.Generic;
using Godot;

namespace ProjectSilicon.Scripts.World;

public static class ProductionSystem
{
    private static readonly MachineType[] ProductionOrder =
    {
        MachineType.SilicaExtractor,
        MachineType.SiliconSmelter,
        MachineType.CrystalGrower
    };

    public static void Update(
        Dictionary<Vector2I, MachineInstance> machines,
        double delta)
    {
        foreach (MachineType machineType in ProductionOrder)
        {
            foreach (MachineInstance machine in machines.Values)
            {
                if (machine.Type != machineType)
                {
                    continue;
                }

                UpdateMachineProduction(machine, delta);
            }
        }
    }

    private static void UpdateMachineProduction(
        MachineInstance machine,
        double delta)
    {
        MachineDefinition definition = MachineDatabase.Get(machine.Type);
        RecipeDefinition recipe = definition.Recipe;

        if (!machine.IsProducing)
        {
            if (!TryStartProduction(machine, recipe))
            {
                return;
            }
        }

        machine.ProductionProgress += delta;

        if (machine.ProductionProgress < recipe.Duration)
        {
            return;
        }

        if (!machine.TryAddOutput( recipe.OutputResource, recipe.OutputAmount))
        {
            machine.IsProducing = false;
            return;
        }
        machine.ProductionProgress = 0.0;
        machine.IsProducing = false;
    }

    private static bool TryStartProduction(
        MachineInstance machine,
        RecipeDefinition recipe)
    {
        int currentOutput =
            machine.GetOutputAmount(recipe.OutputResource);

        if (currentOutput + recipe.OutputAmount > machine.MaxOutput)
        {
            return false;
        }
        
        if (recipe.InputResource is ResourceType inputResource)
        {
            if (!machine.TryConsumeInput(
                    inputResource,
                    recipe.InputAmount))
            {
                return false;
            }
        }
        machine.IsProducing = true;
        return true;
    }
}