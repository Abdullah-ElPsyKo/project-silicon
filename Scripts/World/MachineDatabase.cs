using System.Collections.Generic;
using Godot;

namespace ProjectSilicon.Scripts.World;

public static class MachineDatabase
{
    private static readonly Dictionary<MachineType, MachineDefinition> Definitions =
        new()
        {
            [MachineType.SilicaExtractor] = new MachineDefinition(
                name: "Silica Extractor",
                color: new Color("4385d1"),
                description: "Extracts raw silica."
            ),

            [MachineType.SiliconSmelter] = new MachineDefinition(
                name: "Silicon Smelter",
                color: new Color("d18443"),
                description: "Refines silica into silicon."
            ),

            [MachineType.CrystalGrower] = new MachineDefinition(
                name: "Crystal Grower",
                color: new Color("ffff00"),
                description: "Grows purified silicon into a crystal."
            )
        };
    
    public static MachineDefinition Get(MachineType machineType)
    {
        return Definitions[machineType];
    }
}