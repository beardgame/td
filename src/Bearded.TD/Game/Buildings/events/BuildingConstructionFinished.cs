using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Buildings
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
