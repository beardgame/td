using Bearded.TD.Game.Events;
using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Game.Buildings
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
