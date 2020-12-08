using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Game.GameState.Upgrades;

namespace Bearded.TD.Game.GameState.Buildings
{
    struct BuildingUpgradeFinished : IGlobalEvent
    {
        public Building Building { get; }
        public IUpgradeBlueprint Upgrade { get; }

        public BuildingUpgradeFinished(Building building, IUpgradeBlueprint upgrade)
        {
            Building = building;
            Upgrade = upgrade;
        }
    }
}
