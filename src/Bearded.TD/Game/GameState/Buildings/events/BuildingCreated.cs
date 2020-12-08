using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Buildings
{
    struct BuildingCreated : IGlobalEvent
    {
        public Building Building { get; }

        public BuildingCreated(Building building)
        {
            Building = building;
        }
    }
}
