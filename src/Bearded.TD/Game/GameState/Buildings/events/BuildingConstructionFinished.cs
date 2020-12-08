using Bearded.TD.Game.GameState.Events;

namespace Bearded.TD.Game.GameState.Buildings
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
