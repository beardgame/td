using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Upgrades
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
