using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Projectiles.DamageOnObjectHit;
using static Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("damageOnHit")]
sealed class DamageOnObjectHit(IParameters parameters)
    : Component<IParameters>(parameters), IListener<ObjectHit>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        DamageType? DamageType { get; }

        [Modifiable(defaultValue: 1.0f)]
        float FractionOfBaseDamage { get; }

        bool ExcludeBuildings { get; }

        TimeSpan Delay { get; }
        TimeSpan DelayPerDistanceFromSource { get; }

        [Modifiable(defaultValue: 1)]
        float DamagePotentialConsumptionFraction { get; }
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    protected override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public void HandleEvent(ObjectHit e)
    {
        if (Parameters.ExcludeBuildings && e.Object.TryGetSingleComponent<IBuildingStateProvider>(out _))
            return;

        var delay = Parameters.Delay;

        if (Parameters.DelayPerDistanceFromSource > Zero && Owner.TryGetProperty<Source>(out var source))
        {
            var impactPoint = e.Hit.Impact?.Point ?? e.Object.Position;

            var distance = (source.Object.Position - impactPoint).Length;
            delay += Parameters.DelayPerDistanceFromSource * distance.NumericValue;
        }

        if (delay == Zero)
        {
            dealDamage(e);
        }
        else
        {
            Owner.Game.DelayBy(delay, () => dealDamage(e));
        }
    }

    private void dealDamage(ObjectHit e)
    {
        var actualDamagePotential = e.Hit.DamagePotential * Parameters.FractionOfBaseDamage;

        var result = DamageExecutor.FromObject(Owner).TryDoDamage(
            e.Object,
            actualDamagePotential.Typed(Parameters.DamageType ?? DamageType.Kinetic),
            e.Hit
        );

        var consumedDamageFraction = result.ConsumedDamagePotential / actualDamagePotential;
        var actualDamageConsumed = e.Hit.DamagePotential * consumedDamageFraction;

        Owner.ReduceDamagePotential(actualDamageConsumed * Parameters.DamagePotentialConsumptionFraction);
    }

    public override void Update(TimeSpan elapsedTime) { }
}
