using System.Collections.Generic;
using ProjectSilicon.Scripts.World.Resources;

namespace ProjectSilicon.Scripts.World.Machines;

public sealed class MachineInstance
{
    public MachineType Type { get; }
    
    public double ProductionProgress { get; set; }
    
    public bool IsProducing { get; set; }
    
    public Dictionary<ResourceType, int> InputBuffer { get; } = new();
    
    public Dictionary<ResourceType, int> OutputBuffer { get; } = new();

    public Direction Direction { get; }
    
    public int MaxInput { get; private set; }
	
    public int MaxOutput { get; private set;}
    
    public MachineInstance(MachineType type, Direction direction)
    {
        Type = type;
        Direction = direction;
        MaxInput = MachineDatabase.Get(type).MaxInput;
        MaxOutput = MachineDatabase.Get(type).MaxOutput;
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

    public bool TryAddInput(ResourceType resource, int amount)
    {
        int currentAmount = GetInputAmount(resource);

        if (currentAmount + amount > MaxInput)
            return false;

        InputBuffer[resource] = currentAmount + amount;
        return true;
    }

    public bool TryAddOutput(ResourceType resource, int amount)
    {
        int currentAmount = GetOutputAmount(resource);

        if (currentAmount + amount > MaxOutput)
            return false;

        OutputBuffer[resource] = currentAmount + amount;
        return true;
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
    
    public bool TryConsumeOutput(ResourceType resource, int amount)
    {
        if (GetOutputAmount(resource) < amount)
            return false;

        OutputBuffer[resource] -= amount;
        return true;
    }
}