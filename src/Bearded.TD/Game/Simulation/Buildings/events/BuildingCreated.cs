using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings
{
    readonly struct BuildingCreated : IGlobalEvent
    {
        public Building Building { get; }

        public BuildingCreated(Building building)
        {
            Building = building;
        }
    }
}
