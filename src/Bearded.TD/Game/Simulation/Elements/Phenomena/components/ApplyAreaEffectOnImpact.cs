using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
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
    IListener<CollideWithLevel>, IListener<CollideWithObject>
    where TParameters : IParametersTemplate<TParameters>, IAreaEffectParameters
    where TEffect : IElementalEffect
{
    protected ApplyAreaEffectOnImpact(TParameters parameters) : base(parameters) { }

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

    public override void Update(TimeSpan elapsedTime) { }

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
        var effect = CreateEffect(damage);

        AreaOfEffect.ApplyStatusEffect(Owner.Game, effect, center, Parameters.Range);
    }

    protected abstract TEffect CreateEffect(UntypedDamage damage);
}
