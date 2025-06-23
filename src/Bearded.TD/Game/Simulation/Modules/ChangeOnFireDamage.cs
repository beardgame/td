using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Elements.Phenomena;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using static Bearded.TD.Game.Simulation.Modules.ChangeOnFireDamage;

namespace Bearded.TD.Game.Simulation.Modules;

[Component("changeOnFireDamage")]
sealed class ChangeOnFireDamage(IParameters parameters)
    : Component<IParameters>(parameters), IPreviewListener<PreviewElementalEffectAttempt<OnFire.Effect>>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        [Modifiable(1)]
        double Factor { get; }
    }

    public override void Activate()
    {
        Events.Subscribe(this);
    }

    protected override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public void PreviewEvent(ref PreviewElementalEffectAttempt<OnFire.Effect> e)
    {
        var effect = e.Attempt.Effect with
        {
            DamagePerSecond = e.Attempt.Effect.DamagePerSecond * (float)Parameters.Factor,
        };

        e = e.WithEffect(effect);
    }
}
