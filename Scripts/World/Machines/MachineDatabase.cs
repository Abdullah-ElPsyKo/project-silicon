using System.Collections.Generic;
using Godot;
using ProjectSilicon.Scripts.World.Resources;

namespace ProjectSilicon.Scripts.World.Machines;

public static class MachineDatabase
{
    private static readonly Dictionary<MachineType, MachineDefinition> Definitions =
        new()
        {
            [MachineType.SilicaExtractor] = new MachineDefinition(
                name: "Silica Extractor",
                color: new Color("4385d1"),
                description: "Extracts raw silica.",
                recipe: new RecipeDefinition(
                    inputResource: null,
                    inputAmount: 0,
                    outputAmount: 1,
                    outputResource: ResourceType.Silica,
                    duration: 1.0
                    ),
                maxInput: 5,
                maxOutput: 5
            ),

            [MachineType.SiliconSmelter] = new MachineDefinition(
                name: "Silicon Smelter",
                color: new Color("d18443"),
                description: "Refines silica into silicon.",
                recipe: new RecipeDefinition(
                    inputResource: ResourceType.Silica,
                    inputAmount: 2,
                    outputAmount: 1,
                    outputResource: ResourceType.Silicon,
                    duration: 1.0
                ),
                maxInput: 5,
                maxOutput: 5
            ),

            [MachineType.CrystalGrower] = new MachineDefinition(
                name: "Crystal Grower",
                color: new Color("ffff00"),
                description: "Grows purified silicon into a crystal.",
                recipe: new RecipeDefinition(
                    inputResource: ResourceType.Silicon,
                    inputAmount: 3,
                    outputAmount: 1,
                    outputResource: ResourceType.SiliconCrystal,
                    duration: 1.0
                ),
                maxInput: 5,
                maxOutput: 5
            )
        };
    
    public static MachineDefinition Get(MachineType machineType)
    {
        return Definitions[machineType];
    }
}