using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

[Component("applyOnFireOnImpact")]
sealed class ApplyOnFireOnImpact : ApplyEffectOnImpact<ApplyOnFireOnImpact.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        double Probability { get; }
        double FractionOfBaseDamageApplied { get; }
        TimeSpan EffectDuration { get; }
    }

    protected override double Probability => Parameters.Probability;

    public ApplyOnFireOnImpact(IParameters parameters) : base(parameters) { }

    protected override IElementalEffect CreateEffect(UntypedDamage damage)
    {
        var damagePerSecond =
            new UntypedDamagePerSecond(
                ((int) (Parameters.FractionOfBaseDamageApplied
                    * damage.Amount.NumericValue
                    / Parameters.EffectDuration.NumericValue))
                .HitPoints());
        Owner.TryGetSingleComponent<IDamageSource>(out var damageSource);

        return new OnFire.Effect(damagePerSecond, damageSource, Parameters.EffectDuration);
    }
}
