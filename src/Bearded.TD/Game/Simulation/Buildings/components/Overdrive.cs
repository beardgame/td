using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class Overdrive : Component
{
    private IUpgradeReceipt? upgradeReceipt;

    protected override void OnAdded() {}

    public override void Activate()
    {
        applyUpgrade();
    }

    private void applyUpgrade()
    {
        var ids = Owner.Game.GamePlayIds;
        var fireRate = new ModifyParameterReversibly(
            AttributeType.FireRate, damageModification(ids), UpgradePrerequisites.Empty);
        var damageOverTime = new ModifyParameterReversibly(
            AttributeType.DamageOverTime, damageModification(ids), UpgradePrerequisites.Empty);
        var upgrade = Upgrade.FromEffects(fireRate, damageOverTime);

        upgradeReceipt = Owner.ApplyUpgrade(upgrade);
    }

    private static ModificationWithId damageModification(IdManager ids)
        => new(ids.GetNext<Modification>(), Modification.MultiplyWith(6));

    public override void Update(TimeSpan elapsedTime) { }

    public override void OnRemoved()
    {
        upgradeReceipt?.Rollback();
    }
}
