using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

abstract class ApplyEffectOnImpact<TParameters> : Component<TParameters>, IListener<HitEnemy>
    where TParameters : IParametersTemplate<TParameters>
{
    protected abstract double Probability { get; }

    protected ApplyEffectOnImpact(TParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(HitEnemy @event)
    {
        if (!StaticRandom.Bool(Probability)) return;
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage)) return;
        if (!@event.Enemy.TryGetSingleComponent<IElementSystemEntity>(out var elementSystemEntity)) return;

        var effect = CreateEffect(damage);
        elementSystemEntity.ApplyEffect(effect);
    }

    protected abstract IElementalEffect CreateEffect(UntypedDamage damage);
}
