using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings
{
    readonly struct BuildingConstructionFinished : IGlobalEvent
    {
        public Building Building { get; }

        public BuildingConstructionFinished(Building building)
        {
            Building = building;
        }
    }
}
