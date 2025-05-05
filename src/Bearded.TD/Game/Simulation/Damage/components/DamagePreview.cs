using System;
using System.Collections.Generic;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

struct DamagePreview(TypedDamage damage, List<AdditionalHitEffect> additionalEffects)
{
    // Information about initial hit
    public TypedDamage UnmodifiedDamage { get; } = damage;
    public DamageType DamageType { get; } = damage.Type;
    public HitPoints UnmodifiedDamageAmount { get; } = damage.Amount;

    public Resistance DamageResistance { get; private set; } = Resistance.Zero;
    public HitPoints FlatArmourReduction { get; private set; } = HitPoints.Zero;
    public double ArmourPiercingEffectiveness { get; private set; } = 1.0;
    public HitPoints DamageCap { get; private set; } = HitPoints.Max;
    public double DamageOverCapEffectiveness { get; private set; } = 1.0;

    public void AddAdditionalEffect(AdditionalHitEffect effect)
    {
        additionalEffects.Add(effect);
    }

    public void Resist(Resistance resistance)
    {
        DamageResistance = Resistance.Max(resistance, DamageResistance);
    }

    public void ApplyArmour(HitPoints amount)
    {
        FlatArmourReduction = SpaceTime1MathF.Max(amount, FlatArmourReduction);
    }

    public void PierceArmour(double effectiveness)
    {
        ArmourPiercingEffectiveness = Math.Min(effectiveness, ArmourPiercingEffectiveness);
    }

    public void ApplyDamageCap(HitPoints amount)
    {
        DamageCap = SpaceTime1MathF.Min(amount, DamageCap);
    }

    public void PierceDamageCap(double effectiveness)
    {
        DamageOverCapEffectiveness = Math.Min(effectiveness, DamageOverCapEffectiveness);
    }
}
