using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Game.Simulation.Buildings
{
    readonly struct BuildingUpgradeFinished : IGlobalEvent
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
