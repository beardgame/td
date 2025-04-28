using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

static class DamageCalculations
{
    public static UntypedDamage FlatArmourBonus(
        UntypedDamage initialDamage,
        HitPoints maxBlockedDamage,
        double blockedEffectiveness,
        double piercingFactor)
    {
        var blockedDamage = new UntypedDamage(SpaceTime1MathF.Min(initialDamage.Amount, maxBlockedDamage));
        var passedDamage = initialDamage - blockedDamage;
        var piercedDamage = blockedDamage * (float) piercingFactor;
        blockedDamage -= piercedDamage;
        return blockedDamage * (float) blockedEffectiveness + piercedDamage + passedDamage;
    }
}
