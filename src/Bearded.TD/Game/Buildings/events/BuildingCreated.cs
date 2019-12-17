using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Buildings
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
