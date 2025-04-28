using OpenTK.Mathematics;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class ArmorDamagePreview(
    TypedDamage unmodifiedDamage,
    HitPoints blockedAmount,
    float blockedDamageEffectiveness,
    float piercingFactor)
    : DamagePreview(unmodifiedDamage.Type, unmodifiedDamage.Untyped())
{
    // These numbers do not consider piercing, as this might change, and we want the order in which damage modifiers
    // are applied to not matter, so we need to start from the same base.
    public UntypedDamage DamageAfterArmor { get; } =
        DamageCalculations.FlatArmourBonus(unmodifiedDamage.Untyped(), blockedAmount, blockedDamageEffectiveness, 0);

    public float PiercingFactor { get; private set; } = piercingFactor;

    public void PierceArmor(float percentage)
    {
        PiercingFactor = MathHelper.Clamp(percentage, PiercingFactor, 1);
    }
}
