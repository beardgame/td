using System;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct Resistance(float NumericValue)
{
    public static Resistance Full => new(1);

    public UntypedDamage ApplyToDamage(UntypedDamage d) => (1 - NumericValue) * d;

    public static Resistance operator +(Resistance left, Resistance right) =>
        new(Math.Clamp(left.NumericValue + right.NumericValue, 0, 1));

    public static bool operator <(Resistance left, Resistance right) =>
        left.NumericValue < right.NumericValue;

    public static bool operator <=(Resistance left, Resistance right) =>
        left.NumericValue <= right.NumericValue;

    public static bool operator >(Resistance left, Resistance right) =>
        left.NumericValue > right.NumericValue;

    public static bool operator >=(Resistance left, Resistance right) =>
        left.NumericValue >= right.NumericValue;

    public static Resistance Max(Resistance left, Resistance right) =>
        new(Math.Max(left.NumericValue, right.NumericValue));
}
