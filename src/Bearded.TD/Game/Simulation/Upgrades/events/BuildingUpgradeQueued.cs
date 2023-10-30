using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

readonly struct BuildingUpgradeQueued : IGlobalEvent
{
    public string BuildingName { get; }
    public GameObject GameObject { get; }
    public IIncompleteUpgrade IncompleteUpgrade { get; }
    public IPermanentUpgrade Upgrade => IncompleteUpgrade.Upgrade;

    public BuildingUpgradeQueued(string buildingName, GameObject gameObject, IIncompleteUpgrade incompleteUpgrade)
    {
        BuildingName = buildingName;
        GameObject = gameObject;
        IncompleteUpgrade = incompleteUpgrade;
    }
}
