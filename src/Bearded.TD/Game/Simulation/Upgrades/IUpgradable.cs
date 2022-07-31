namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradable
{
    bool CanApplyUpgrade(IPermanentUpgrade upgrade);
}
