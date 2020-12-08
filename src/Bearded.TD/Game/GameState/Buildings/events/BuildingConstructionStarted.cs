using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Buildings
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
