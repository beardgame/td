using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

abstract class ApplyEffectOnHit<TParameters, TEffect>(TParameters parameters) : Component<TParameters>(parameters),
    IListener<ObjectHit>
    where TParameters : IParametersTemplate<TParameters>
    where TEffect : IElementalEffect<TEffect>
{
    protected abstract double Probability { get; }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(ObjectHit @event)
    {
        var effect = CreateEffect(@event.Hit.DamagePotential);
        @event.Object.TryApplyEffect(effect, Probability);
    }

    protected abstract TEffect CreateEffect(UntypedDamage damage);
}
