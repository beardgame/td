using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings
{
    struct BuildingConstructionFinished : IGlobalEvent
    {
        public Building Building { get; }

        public BuildingConstructionFinished(Building building)
        {
            Building = building;
        }
    }
}
