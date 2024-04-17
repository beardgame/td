using System;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Resources;

[FactionBehavior("resources")]
sealed class FactionResources : FactionBehavior
{
    public ResourceAmount CurrentResources { get; private set; }

    public ResourceAmount AvailableResources => CurrentResources;

    protected override void Execute() {}

    public void ProvideResources(ResourceAmount amount)
    {
        CurrentResources += amount;
    }

    public void ConsumeResources(ResourceAmount amount)
    {
        if (AvailableResources < amount)
        {
            throw new InvalidOperationException("Not enough resources available.");
        }
        CurrentResources -= amount;
    }
}
