using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Upgrades;

readonly struct BuildingUpgradeFinished : IGlobalEvent
{
    public string BuildingName { get; }
    public IGameObject GameObject { get; }
    public IPermanentUpgrade PermanentUpgrade { get; }

    public BuildingUpgradeFinished(string buildingName, IGameObject gameObject, IPermanentUpgrade upgrade)
    {
        BuildingName = buildingName;
        GameObject = gameObject;
        PermanentUpgrade = upgrade;
    }
}