using Bearded.TD.Game.Buildings;

namespace Bearded.TD.Game.Events
{
    struct BuildingConstructionStarted : IEvent
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
