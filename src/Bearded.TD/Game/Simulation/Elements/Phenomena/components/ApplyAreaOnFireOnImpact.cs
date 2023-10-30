using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

[Component("applyAreaOnFireOnImpact")]
sealed class ApplyAreaOnFireOnImpact : ApplyAreaEffectOnImpact<ApplyAreaOnFireOnImpact.IParameters, OnFire.Effect>
{
    public interface IParameters : IParametersTemplate<IParameters>, IAreaEffectParameters
    {
        double FractionOfBaseDamage { get; }
        TimeSpan EffectDuration { get; }
    }

    public ApplyAreaOnFireOnImpact(IParameters parameters) : base(parameters) { }

    protected override OnFire.Effect CreateEffect(UntypedDamage damage)
    {
        var damagePerSecond =
            new UntypedDamagePerSecond(
                ((float) (Parameters.FractionOfBaseDamage
                    * damage.Amount.NumericValue
                    / Parameters.EffectDuration.NumericValue))
                .HitPoints());
        Owner.TryGetSingleComponent<IDamageSource>(out var damageSource);

        return new OnFire.Effect(damagePerSecond, damageSource, Parameters.EffectDuration);
    }
}
