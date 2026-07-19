using System.Collections.Generic;

namespace ProjectSilicon.Scripts.World;

public sealed class MachineInstance
{
    public MachineType Type { get; }
    
    public double ProductionProgress { get; set; }
    
    public bool IsProducing { get; set; }
    
    public MachineInstance(MachineType type)
    {
        Type = type;
    }
}