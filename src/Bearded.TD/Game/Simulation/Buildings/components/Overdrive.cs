using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.Weapons;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings;

sealed class Overdrive : Component
{
    private static readonly TimeSpan damageInterval = 0.5.S();
    private static float damagePercentile = 0.025f;

    private IUpgradeEffect fireRate = null!;
    private IUpgradeEffect damageOverTime = null!;
    private ComponentDependencies.IDependencyRef weaponTriggerDependency = null!;

    private IWeaponTrigger? trigger;
    private bool wasTriggerPulledLastFrame;
    private Instant nextDamageTime;

    protected override void OnAdded() {}

    public override void Activate()
    {
        base.Activate();

        var ids = Owner.Game.GamePlayIds;

        fireRate = new ParameterModifiableWithId(
            AttributeType.FireRate, damageModification(ids), UpgradePrerequisites.Empty);
        damageOverTime = new ParameterModifiableWithId(
            AttributeType.DamageOverTime, damageModification(ids), UpgradePrerequisites.Empty);

        fireRate.ApplyTo(Owner);
        damageOverTime.ApplyTo(Owner);

        weaponTriggerDependency = ComponentDependencies
            .DependDynamic<IWeaponTrigger>(Owner, Events, t => trigger = t);
    }

    private static ModificationWithId damageModification(IdManager ids)
        => new(ids.GetNext<Modification>(), Modification.MultiplyWith(1.5));

    public override void OnRemoved()
    {
        fireRate.RemoveFrom(Owner);
        damageOverTime.RemoveFrom(Owner);
        weaponTriggerDependency.Dispose();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        var triggerIsPulled = trigger?.TriggerPulled ?? false;

        if (triggerIsPulled && nextDamageTime <= Owner.Game.Time)
        {
            damageOwner();

            var timeOfDamage = wasTriggerPulledLastFrame ? nextDamageTime : Owner.Game.Time;
            nextDamageTime = timeOfDamage + damageInterval;
        }

        wasTriggerPulledLastFrame = triggerIsPulled;
    }

    private void damageOwner()
    {
        var damage = new HitPoints(
            Owner.TryGetSingleComponent<IHealth>(out var health)
                ? MoreMath.CeilToInt(health.MaxHealth.NumericValue * damagePercentile)
                : 10);

        var overdriveDamage = new TypedDamage(damage, DamageType.DivineIntervention);

        DamageExecutor.FromDamageSource(null).TryDoDamage(Owner, overdriveDamage);

    }
}
