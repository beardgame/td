using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Buildings
{
    readonly struct BuildingConstructionStarted : IGlobalEvent
    {
        public BuildingPlaceholder Placeholder { get; }
        public Building Building { get; }

        public BuildingConstructionStarted(BuildingPlaceholder placeholder, Building building)
        {
            Placeholder = placeholder;
            Building = building;
        }
    }
}
