using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class Overdrive : Component
{
    private readonly IUpgrade upgrade;
    private IUpgradeReceipt? upgradeReceipt;

    public Overdrive(IUpgrade upgrade)
    {
        this.upgrade = upgrade;
    }

    protected override void OnAdded() {}

    public override void Activate()
    {
        applyUpgrade();
    }

    private void applyUpgrade()
    {
        upgradeReceipt = Owner.ApplyUpgrade(upgrade);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public override void OnRemoved()
    {
        upgradeReceipt?.Rollback();
    }
}
