using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

[Component("applyOnFireOnImpact")]
sealed class ApplyOnFireOnHit : ApplyEffectOnHit<ApplyOnFireOnHit.IParameters, OnFire.Effect>
{
    public interface IParameters : IParametersTemplate<IParameters>
    {
        double Probability { get; }
        double FractionOfBaseDamage { get; }
        TimeSpan EffectDuration { get; }
    }

    protected override double Probability => Parameters.Probability;

    public ApplyOnFireOnHit(IParameters parameters) : base(parameters) { }

    protected override OnFire.Effect CreateEffect(UntypedDamage damage)
    {
        var damagePerSecond =
            new UntypedDamagePerSecond(
                ((float) (Parameters.FractionOfBaseDamage
                    * damage.Amount.NumericValue
                    / Parameters.EffectDuration.NumericValue))
                .HitPoints());
        Owner.TryGetSingleComponentInOwnerTree<IDamageSource>(out var damageSource);

        return new OnFire.Effect(damagePerSecond, damageSource, Parameters.EffectDuration);
    }
}
