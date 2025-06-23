using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

struct DamagePreview(TypedDamage damage)
{
    public UntypedDamage DamageAmount { get; private set; } = damage.Untyped();
    public DamageType DamageType { get; private set; } = damage.Type;

    public UntypedDamage DamagePotentialConsumed { get; private set; } = damage.Untyped();

    public UntypedDamage PiercingDamageAmount { get; private set; }

    public void Resist(Resistance resistance)
    {
        DamageAmount = resistance.ApplyToDamage(DamageAmount);
    }

    public void ReduceDamageUnderThreshold(UntypedDamage threshold, double effectivenessUnderThreshold)
    {
        var underThreshold = SpaceTime1MathF.Min(threshold, DamageAmount);
        var overThreshold = DamageAmount - underThreshold;
        DamageAmount = overThreshold + (float) effectivenessUnderThreshold * underThreshold;
    }

    public void ReduceDamageOverThreshold(UntypedDamage threshold, double effectivenessOverThreshold)
    {
        var underThreshold = SpaceTime1MathF.Min(threshold, DamageAmount);
        var overThreshold = DamageAmount - underThreshold;
        DamageAmount = underThreshold + (float) effectivenessOverThreshold * overThreshold;
    }

    public void PierceDamageToNextShell(double piercingFraction)
    {
        var piercingDamage = DamageAmount * (float) piercingFraction;
        PiercingDamageAmount += piercingDamage;
        DamageAmount -= piercingDamage;
        DamagePotentialConsumed -= piercingDamage;
    }
}
