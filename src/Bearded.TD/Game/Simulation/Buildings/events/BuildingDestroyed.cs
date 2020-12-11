using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings
{
    struct BuildingDestroyed : IGlobalEvent
    {
        public Building Builder { get; }

        public BuildingDestroyed(Building building)
        {
            Builder = building;
        }
    }
}
