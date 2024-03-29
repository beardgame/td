using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("splashDamageOnHit")]
sealed class SplashDamageOnHit : Component<SplashDamageOnHit.IParameters>,
    IListener<CollideWithLevel>, IListener<CollideWithObject>
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
        Events.Subscribe<CollideWithLevel>(this);
        Events.Subscribe<CollideWithObject>(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<CollideWithLevel>(this);
        Events.Unsubscribe<CollideWithObject>(this);
    }

    public void HandleEvent(CollideWithLevel @event)
    {
        onHit(@event.Info.Point);
    }

    public void HandleEvent(CollideWithObject @event)
    {
        onHit(@event.Impact.Point);
    }

    private void onHit(Position3 center)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var unadjustedDamage))
        {
            DebugAssert.State.IsInvalid();
            return;
        }

        var damage = new UntypedDamage(
            (unadjustedDamage.Amount.NumericValue / Parameters.DamageDivisionFactor).HitPoints());
        var typedDamage = damage.Typed(Parameters.DamageType ?? DamageType.Kinetic);

        AreaOfEffect.Damage(
            Owner.Game, DamageExecutor.FromObject(Owner), typedDamage, center, Parameters.Range);
    }

    public override void Update(TimeSpan elapsedTime) { }
}
