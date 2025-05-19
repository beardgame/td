using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

abstract class ApplyEffectOnImpact<TParameters, TEffect>(TParameters parameters) : Component<TParameters>(parameters),
    IListener<TouchObject>
    where TParameters : IParametersTemplate<TParameters>
    where TEffect : IElementalEffect<TEffect>
{
    protected abstract double Probability { get; }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(TouchObject @event)
    {
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage)) return;

        var effect = CreateEffect(damage);
        @event.Object.TryApplyEffect(effect, Probability);
    }

    protected abstract TEffect CreateEffect(UntypedDamage damage);
}
