using System;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct Resistance
{
    public static Resistance Zero => new(0);

    // Represents the percentage damage is reduced by
    public float NumericValue { get; }

    public Resistance(float numericValue)
    {
        Argument.IsFraction(numericValue);
        NumericValue = numericValue;
    }

    public UntypedDamage ApplyToDamage(UntypedDamage d) => (1 - NumericValue) * d;
    public TypedDamage ApplyToDamage(TypedDamage d) => (1 - NumericValue) * d;

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
}
