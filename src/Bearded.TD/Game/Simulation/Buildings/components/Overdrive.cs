using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class Overdrive : Component
{
    private static readonly TimeSpan damageInterval = 0.5.S();
    private const float damagePercentile = 0.025f;

    private IUpgradeReceipt? upgradeReceipt;

    private Instant nextDamageTime;

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();

        var ids = Owner.Game.GamePlayIds;

        var fireRate = new ParameterModifiableWithId(
            AttributeType.FireRate, damageModification(ids), UpgradePrerequisites.Empty);
        var damageOverTime = new ParameterModifiableWithId(
            AttributeType.DamageOverTime, damageModification(ids), UpgradePrerequisites.Empty);
        var upgrade = Upgrade.FromEffects(fireRate, damageOverTime);

        upgradeReceipt = Owner.ApplyUpgrade(upgrade);
        nextDamageTime = Owner.Game.Time;
    }

    private static ModificationWithId damageModification(IdManager ids)
        => new(ids.GetNext<Modification>(), Modification.MultiplyWith(1.5));

    public override void OnRemoved()
    {
        upgradeReceipt?.Rollback();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (nextDamageTime > Owner.Game.Time)
        {
            return;
        }

        damageOwner();

        nextDamageTime += damageInterval;
    }

    private void damageOwner()
    {
        var damage = new HitPoints(
            Owner.TryGetSingleComponent<IHealth>(out var health)
                ? MoreMath.CeilToInt(health.MaxHealth.NumericValue * damagePercentile)
                : 10);

        var overdriveDamage = new TypedDamage(damage, DamageType.DivineIntervention);

        DamageExecutor.FromDamageSource(null).TryDoDamage(Owner, overdriveDamage, Hit.FromSelf());

    }
}
