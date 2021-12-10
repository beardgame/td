namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradable
{
    bool CanApplyUpgrade(IUpgradeBlueprint upgrade);
}