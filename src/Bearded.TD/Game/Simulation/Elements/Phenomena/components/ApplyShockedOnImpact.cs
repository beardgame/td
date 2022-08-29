using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

[Component("applyShockedOnImpact")]
sealed class ApplyShockedOnImpact : ApplyEffectOnImpact<ApplyShockedOnImpact.IParameters, Shocked.Effect>
{
    // Note on defaults:
    //   The defaults are chosen so that a projectile being shot once per second - doing exactly 30 damage - would lead
    //   to the enemy being slowed by 50% for 0.6 seconds. For this duration, the damage potential for the enemy per
    //   tile moved is doubled. If the enemy was successfully shocked each hit, this would lead to an overall increase
    //   of damage _taken_ by the enemy by 60%. The effectiveness therefore scales with the number of towers able to
    //   shoot the enemy at a given time.
    public interface IParameters : IParametersTemplate<IParameters>
    {
        double Probability { get; }

        [Modifiable(0.02)]
        double DamageToDurationFactor { get; }

        [Modifiable(0.5)]
        double MovementPenalty { get; }
    }

    protected override double Probability => Parameters.Probability;

    public ApplyShockedOnImpact(IParameters parameters) : base(parameters) { }

    protected override Shocked.Effect CreateEffect(UntypedDamage damage)
    {
        var duration = new TimeSpan(Parameters.DamageToDurationFactor * damage.Amount.NumericValue);
        return new Shocked.Effect(Parameters.MovementPenalty, duration);
    }
}
