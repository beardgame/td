using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings;

readonly struct BuildingDestroyed : IGlobalEvent
{
    public IComponentOwner Building { get; }

    public BuildingDestroyed(IComponentOwner building)
    {
        Building = building;
    }
}