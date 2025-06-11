using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Game.Simulation.Projectiles.DamageOnObjectHit;
using static Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

readonly record struct ObjectHit(Hit Hit, GameObject Object) : IComponentEvent;

static class Hits
{
    public static void HitObject(GameObject subject, ComponentEvents events, GameObject obj, Impact impact)
    {
        var potential = subject.TryGetProperty<UntypedDamage>(out var damage)
            ? damage : UntypedDamage.Zero;

        var hit = Hit.FromImpact(impact, potential);

        events.Send(new ObjectHit(hit, obj));
    }
}

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
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    protected override void OnRemovedInternal()
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
        _ = DamageExecutor.FromObject(Owner).TryDoDamage(
            e.Object,
            (e.Hit.DamagePotential * Parameters.FractionOfBaseDamage).Typed(Parameters.DamageType ?? DamageType.Kinetic),
            e.Hit
        );
    }

    public override void Update(TimeSpan elapsedTime) { }
}
