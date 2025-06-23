using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("splashDamageOnHit")]
sealed class SplashDamageOnHit : Component<SplashDamageOnHit.IParameters>,
    IListener<CollidedWithLevel>, IListener<ObjectHit>
{
    internal interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }

        [Modifiable(3)]
        int DamageDivisionFactor { get; }

        DamageType? DamageType { get; }
    }

    public SplashDamageOnHit(IParameters parameters) : base(parameters) {}

    protected override void OnAdded()
    {
        Events.Subscribe<CollidedWithLevel>(this);
        Events.Subscribe<ObjectHit>(this);
    }

    protected override void OnRemoved()
    {
        Events.Unsubscribe<CollidedWithLevel>(this);
        Events.Unsubscribe<ObjectHit>(this);
    }

    public void HandleEvent(CollidedWithLevel @event)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage))
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        onHit(@event.Info.Point, damage);
    }

    public void HandleEvent(ObjectHit e)
    {
        onHit(e.Hit.Impact?.Point ?? Owner.Position, e.Hit.DamagePotential);
    }

    private void onHit(Position3 center, UntypedDamage damage)
    {
        damage = new UntypedDamage(
            (damage.Amount.NumericValue / Parameters.DamageDivisionFactor).HitPoints());
        var typedDamage = damage.Typed(Parameters.DamageType ?? DamageType.Kinetic);

        AreaOfEffect.Damage(
            Owner.Game, DamageExecutor.FromObject(Owner), typedDamage, center, Parameters.Range);
    }

    public override void Update(TimeSpan elapsedTime) { }
}
