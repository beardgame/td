﻿using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Buildings;

readonly struct BuildingDestroyed : IGlobalEvent
{
    public IComponentOwner Building { get; }

    public BuildingDestroyed(IComponentOwner building)
    {
        Building = building;
    }
}
