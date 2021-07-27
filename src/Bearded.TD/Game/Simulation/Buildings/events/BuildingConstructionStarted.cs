using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings
{
    readonly struct BuildingConstructionStarted : IGlobalEvent
    {
        public IComponentOwner Building { get; }

        public BuildingConstructionStarted(IComponentOwner building)
        {
            Building = building;
        }
    }
}
