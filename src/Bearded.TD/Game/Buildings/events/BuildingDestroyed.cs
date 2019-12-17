using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Buildings
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
