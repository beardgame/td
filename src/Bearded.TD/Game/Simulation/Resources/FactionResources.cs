﻿using System;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Resources;

[FactionBehavior("resources")]
sealed class FactionResources : FactionBehavior
{
    public ResourceAmount CurrentResources { get; private set; }

    protected override void Execute() {}

    public void ProvideResources(ResourceAmount amount)
    {
        CurrentResources += amount;
        Events.Send(new ResourcesProvided(this, amount));
    }

    public void ConsumeResources(ResourceAmount amount)
    {
        if (CurrentResources < amount)
        {
            throw new InvalidOperationException("Not enough resources available.");
        }
        CurrentResources -= amount;
        Events.Send(new ResourcesConsumed(this, amount));
    }
}
