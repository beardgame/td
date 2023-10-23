using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;
using static Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("damageOnHit")]
sealed class DamageOnHit : Component<DamageOnHit.IParameters>, IListener<TouchObject>
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

    public DamageOnHit(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public void HandleEvent(TouchObject e)
    {
        if (Parameters.ExcludeBuildings && e.Object.TryGetSingleComponent<IBuildingStateProvider>(out _))
            return;

        var delay = Parameters.Delay;

        if (Parameters.DelayPerDistanceFromSource > Zero && Owner.TryGetProperty<Source>(out var source))
        {
            var distance = (source.Object.Position - e.Impact.Point).Length;
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

    private void dealDamage(TouchObject e)
    {
        _ = Owner.TryGetProperty<UntypedDamage>(out var damage)
            && DamageExecutor.FromObject(Owner).TryDoDamage(
                e.Object,
                (damage * Parameters.FractionOfBaseDamage).Typed(Parameters.DamageType ?? DamageType.Kinetic),
                Hit.FromImpact(e.Impact));
    }

    public override void Update(TimeSpan elapsedTime) { }
}
