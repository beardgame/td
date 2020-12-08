using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Buildings
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
