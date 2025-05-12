using System.Collections.Generic;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

struct DamagePreview(TypedDamage damage, List<AdditionalHitEffect> additionalEffects)
{
    public HitPoints DamageAmount { get; private set; } = damage.Amount;
    public DamageType DamageType { get; private set; } = damage.Type;

    public void AddAdditionalEffect(AdditionalHitEffect effect)
    {
        additionalEffects.Add(effect);
    }

    public void Resist(Resistance resistance)
    {
        DamageAmount = resistance.ApplyToDamage(new UntypedDamage(DamageAmount)).Amount;
    }

    public void ReduceDamageUnderThreshold(HitPoints threshold, double effectivenessUnderThreshold)
    {
        var underThreshold = SpaceTime1MathF.Min(threshold, DamageAmount);
        var overThreshold = DamageAmount - underThreshold;
        DamageAmount = overThreshold + (float) effectivenessUnderThreshold * underThreshold;
    }

    public void ReduceDamageOverThreshold(HitPoints threshold, double effectivenessOverThreshold)
    {
        var underThreshold = SpaceTime1MathF.Min(threshold, DamageAmount);
        var overThreshold = DamageAmount - underThreshold;
        DamageAmount = underThreshold + (float) effectivenessOverThreshold * overThreshold;
    }
}
