using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    readonly struct BuildingUpgradeQueued : IGlobalEvent
    {
        public string BuildingName { get; }
        public IGameObject GameObject { get; }
        public IIncompleteUpgrade IncompleteUpgrade { get; }
        public IUpgradeBlueprint Upgrade => IncompleteUpgrade.Upgrade;

        public BuildingUpgradeQueued(string buildingName, IGameObject gameObject, IIncompleteUpgrade incompleteUpgrade)
        {
            BuildingName = buildingName;
            GameObject = gameObject;
            IncompleteUpgrade = incompleteUpgrade;
        }
    }
}
