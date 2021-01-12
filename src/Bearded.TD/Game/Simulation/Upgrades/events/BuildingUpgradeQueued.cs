using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    readonly struct BuildingUpgradeQueued : IGlobalEvent
    {
        public Building Building { get; }
        public BuildingUpgradeTask Task { get; }
        public IUpgradeBlueprint Upgrade => Task.Upgrade;

        public BuildingUpgradeQueued(Building building, BuildingUpgradeTask task)
        {
            Building = building;
            Task = task;
        }
    }
}
