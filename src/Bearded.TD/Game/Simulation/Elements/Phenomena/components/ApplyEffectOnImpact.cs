using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

abstract class ApplyEffectOnImpact<TParameters, TEffect> : Component<TParameters>, IListener<TouchObject>
    where TParameters : IParametersTemplate<TParameters>
    where TEffect : IElementalEffect
{
    protected abstract double Probability { get; }

    protected ApplyEffectOnImpact(TParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(TouchObject @event)
    {
        if (!StaticRandom.Bool(Probability)) return;
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage)) return;
        if (!@event.Object.TryGetSingleComponent<IElementSystemEntity>(out var elementSystemEntity)) return;

        var effect = CreateEffect(damage);
        elementSystemEntity.ApplyEffect(effect);
    }

    protected abstract TEffect CreateEffect(UntypedDamage damage);
}
