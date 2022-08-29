using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

[Component("applyShockedOnImpact")]
sealed class ApplyShockedOnImpact : ApplyEffectOnImpact<ApplyShockedOnImpact.IParameters>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        double Probability { get; }
        double FractionOfBaseDamageConvertedToPenalty { get; }
        TimeSpan EffectDuration { get; }
    }

    protected override double Probability => Parameters.Probability;

    public ApplyShockedOnImpact(IParameters parameters) : base(parameters) { }

    protected override IElementalEffect CreateEffect(UntypedDamage damage)
    {
        var damagePerSecond =
            new UntypedDamagePerSecond(
                ((int) (Parameters.FractionOfBaseDamageConvertedToPenalty
                    * damage.Amount.NumericValue
                    / Parameters.EffectDuration.NumericValue))
                .HitPoints());
        var penalty = ShockedMovementPenalty.ToMovementPenalty(damagePerSecond);

        return new Shocked.Effect(penalty, Parameters.EffectDuration);
    }
}
