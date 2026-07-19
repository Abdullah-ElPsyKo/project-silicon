using System.Collections.Generic;

namespace ProjectSilicon.Scripts.World;

public sealed class MachineInstance
{
    public MachineType Type { get; }

    public Dictionary<ResourceType, int> InputBuffer { get; } = new();

    public Dictionary<ResourceType, int> OutputBuffer { get; } = new();

    public MachineInstance(MachineType type)
    {
        Type = type;
    }
}