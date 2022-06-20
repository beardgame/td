using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct UntypedDamagePerSecond
{
    public HitPoints Amount { get; }

    public static UntypedDamagePerSecond Zero => new(HitPoints.Zero);

    public UntypedDamagePerSecond(HitPoints amount)
    {
        Argument.Satisfies(amount >= HitPoints.Zero);
        Amount = amount;
    }

    public static UntypedDamage operator /(UntypedDamagePerSecond dps, Frequency fireRate)
    {
        return new UntypedDamage(
            StaticRandom.Discretise(dps.Amount.NumericValue / (float) fireRate.NumericValue).HitPoints());
    }
}
