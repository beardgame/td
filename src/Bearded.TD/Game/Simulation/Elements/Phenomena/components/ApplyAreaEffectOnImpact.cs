using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

interface IAreaEffectParameters
{
    [Modifiable(Type = AttributeType.SplashRange)]
    Unit Range { get; }

    [Modifiable(3)]
    int DamageDivisionFactor { get; }
}

abstract class ApplyAreaEffectOnImpact<TParameters, TEffect> : Component<TParameters>,
    IListener<CollidedWithLevel>, IListener<ObjectHit>
    where TParameters : IParametersTemplate<TParameters>, IAreaEffectParameters
    where TEffect : IElementalEffect<TEffect>
{
    protected ApplyAreaEffectOnImpact(TParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe<CollidedWithLevel>(this);
        Events.Subscribe<ObjectHit>(this);
    }

    protected override void OnRemovedInternal()
    {
        Events.Unsubscribe<CollidedWithLevel>(this);
        Events.Unsubscribe<ObjectHit>(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

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
        var effect = CreateEffect(damage);

        AreaOfEffect.ApplyStatusEffect(Owner.Game, effect, center, Parameters.Range);
    }

    protected abstract TEffect CreateEffect(UntypedDamage damage);
}
