using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

readonly struct BuildingUpgradeCancelled : IGlobalEvent
{
    public string BuildingName { get; }
    public GameObject GameObject { get; }
    public IPermanentUpgrade Upgrade { get; }

    public BuildingUpgradeCancelled(string buildingName, GameObject gameObject, IPermanentUpgrade upgrade)
    {
        BuildingName = buildingName;
        GameObject = gameObject;
        Upgrade = upgrade;
    }
}
