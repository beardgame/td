using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Shared.Events;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

[Component("applyOnFireOnImpact")]
sealed class ApplyOnFireOnImpact : Component<ApplyOnFireOnImpact.IParameters>, IListener<HitEnemy>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        double Probability { get; }
        double FractionOfBaseDamageApplied { get; }
        TimeSpan EffectDuration { get; }
    }

    public ApplyOnFireOnImpact(IParameters parameters) : base(parameters) { }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void Update(TimeSpan elapsedTime) { }

    public void HandleEvent(HitEnemy @event)
    {
        if (!StaticRandom.Bool(Parameters.Probability)) return;
        if (!Owner.TryGetProperty<UntypedDamage>(out var damage)) return;
        if (!@event.Enemy.TryGetSingleComponent<IElementSystemEntity>(out var elementSystemEntity)) return;

        var damagePerSecond =
            new UntypedDamagePerSecond(
                ((int) (Parameters.FractionOfBaseDamageApplied
                    * damage.Amount.NumericValue
                    / Parameters.EffectDuration.NumericValue))
                .HitPoints());
        Owner.TryGetSingleComponent<IDamageSource>(out var damageSource);

        var effect = new OnFire.Effect(damagePerSecond, damageSource, Parameters.EffectDuration);
        elementSystemEntity.ApplyEffect(effect);
    }
}
