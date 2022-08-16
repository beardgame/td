using Bearded.TD.Game.Simulation.Resources;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class BuildingUpgradeWork : BuildingWork
{
    private readonly IIncompleteUpgrade incompleteUpgrade;

    protected override ResourceAmount Cost => incompleteUpgrade.Upgrade.Cost;
    protected override ResourceRate ConsumptionRate => Constants.Game.Resources.UpgradeSpeed;

    public BuildingUpgradeWork(IIncompleteUpgrade incompleteUpgrade) : base(incompleteUpgrade)
    {
        this.incompleteUpgrade = incompleteUpgrade;
    }
}
