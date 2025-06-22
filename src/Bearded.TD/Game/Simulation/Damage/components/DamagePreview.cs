using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

struct DamagePreview(TypedDamage damage)
{
    // TODO: use UntypedDamage instead
    public HitPoints DamageAmount { get; private set; } = damage.Amount;
    public DamageType DamageType { get; private set; } = damage.Type;

    public UntypedDamage DamagePotentialConsumed { get; private set; } = damage.Untyped();

    // TODO: use UntypedDamage instead
    public HitPoints PiercingDamageAmount { get; private set; }

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

    public void PierceDamageToNextShell(double piercingFraction)
    {
        var piercingDamage = DamageAmount * (float) piercingFraction;
        PiercingDamageAmount += piercingDamage;
        DamageAmount -= piercingDamage;
        DamagePotentialConsumed -= new UntypedDamage(piercingDamage);
    }
}
