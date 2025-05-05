using static Bearded.TD.Utilities.SpaceTime.SpaceTime1MathF;

namespace Bearded.TD.Game.Simulation.Damage;

static class DamageCalculations
{
    public static TypedDamage FromPreview(DamagePreview preview)
    {
        // Shield
        var cappedDamage = Min(preview.UnmodifiedDamageAmount, preview.DamageCap);
        var damageOverCap = preview.UnmodifiedDamageAmount - cappedDamage;
        var damageDoneOverCap = damageOverCap * (float) preview.DamageOverCapEffectiveness;

        // Armour
        var armoredDamage = Min(cappedDamage, preview.FlatArmourReduction);
        var piercedDamageDone = armoredDamage * (float) preview.ArmourPiercingEffectiveness;

        var fullDamageDone = cappedDamage - armoredDamage;

        var damageDoneBeforeResistance = new UntypedDamage(fullDamageDone + piercedDamageDone + damageDoneOverCap);
        var damageDoneAfterResistance = preview.DamageResistance.ApplyToDamage(damageDoneBeforeResistance);

        return damageDoneAfterResistance.Typed(preview.DamageType);
    }
}
