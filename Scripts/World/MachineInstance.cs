using System.Collections.Generic;

namespace ProjectSilicon.Scripts.World;

public sealed class MachineInstance
{
    public MachineType Type { get; }
    
    public double ProductionProgress { get; set; }
    
    public bool IsProducing { get; set; }
    
    public Dictionary<ResourceType, int> InputBuffer { get; } = new();
    
    public Dictionary<ResourceType, int> OutputBuffer { get; } = new();
    
    public MachineInstance(MachineType type)
    {
        Type = type;
    }

    public int GetInputAmount(ResourceType resource)
    {
        InputBuffer.TryGetValue(resource, out int amount);
        return amount;
    }

    public int GetOutputAmount(ResourceType resource)
    {
        OutputBuffer.TryGetValue(resource, out int amount);
        return amount;
    }

    public void AddInput(ResourceType resource, int amount)
    {
        if (InputBuffer.ContainsKey(resource))
        {
            InputBuffer[resource] += amount;
            return;
        }
        InputBuffer[resource] = amount;
    }

    public void AddOutput(ResourceType resource, int amount)
    {
        if (OutputBuffer.ContainsKey(resource))
        {
            OutputBuffer[resource] += amount;
            return;
        }
        OutputBuffer[resource] = amount;
    }

    public bool TryConsumeInput(ResourceType resource, int amount)
    {
        if (GetInputAmount(resource) >= amount)
        {
            InputBuffer[resource] -= amount;
            return true;
        }

        return false;
    }
}