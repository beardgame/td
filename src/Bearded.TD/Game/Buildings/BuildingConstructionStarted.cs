using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Buildings
{
    struct BuildingConstructionStarted : IGlobalEvent
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
