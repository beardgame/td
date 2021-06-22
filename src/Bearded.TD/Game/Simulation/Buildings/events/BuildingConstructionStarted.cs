using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings
{
    readonly struct BuildingConstructionStarted : IGlobalEvent
    {
        public Building Building { get; }

        public BuildingConstructionStarted(Building building)
        {
            Building = building;
        }
    }
}
