namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradeListener
{
    void OnUpgradeCommitted(IUpgrade upgrade);
    void OnUpgradeRolledBack(IUpgrade upgrade);
}
