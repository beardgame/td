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

    public void ApplyFlatReduction(HitPoints threshold, double blockedEffectiveness)
    {
        var underThreshold = SpaceTime1MathF.Min(threshold, DamageAmount);
        var overThreshold = DamageAmount - underThreshold;
        DamageAmount = overThreshold + (float) blockedEffectiveness * underThreshold;
    }

    public void ApplyDamageCap(HitPoints threshold, double effectivenessOverCap)
    {
        var underThreshold = SpaceTime1MathF.Min(threshold, DamageAmount);
        var overThreshold = DamageAmount - underThreshold;
        DamageAmount = underThreshold + (float) effectivenessOverCap * overThreshold;
    }
}
