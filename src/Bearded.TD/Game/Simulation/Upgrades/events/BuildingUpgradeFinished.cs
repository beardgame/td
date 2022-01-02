using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Upgrades;

readonly struct BuildingUpgradeFinished : IGlobalEvent
{
    public string BuildingName { get; }
    public IGameObject GameObject { get; }
    public IUpgradeBlueprint Upgrade { get; }

    public BuildingUpgradeFinished(string buildingName, IGameObject gameObject, IUpgradeBlueprint upgrade)
    {
        BuildingName = buildingName;
        GameObject = gameObject;
        Upgrade = upgrade;
    }
}