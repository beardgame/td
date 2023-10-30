using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

readonly struct BuildingUpgradeFinished : IGlobalEvent
{
    public string BuildingName { get; }
    public GameObject GameObject { get; }
    public IPermanentUpgrade Upgrade { get; }

    public BuildingUpgradeFinished(string buildingName, GameObject gameObject, IPermanentUpgrade upgrade)
    {
        BuildingName = buildingName;
        GameObject = gameObject;
        Upgrade = upgrade;
    }
}
