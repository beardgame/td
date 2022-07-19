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

    public static UntypedDamage operator *(TimeSpan timeSpan, UntypedDamagePerSecond dps) => dps * timeSpan;

    public static UntypedDamage operator *(UntypedDamagePerSecond dps, TimeSpan timeSpan) =>
        new(((int) (dps.Amount.NumericValue * timeSpan.NumericValue)).HitPoints());

    public static UntypedDamage operator /(UntypedDamagePerSecond dps, Frequency fireRate) =>
        new(((int) (dps.Amount.NumericValue / fireRate.NumericValue)).HitPoints());
}
