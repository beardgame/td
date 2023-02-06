using System;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct Resistance
{
    // Represents the percentage damage is reduced by
    private readonly float numericValue;

    public Resistance(float numericValue)
    {
        Argument.IsFraction(numericValue);
        this.numericValue = numericValue;
    }

    public UntypedDamage ApplyToDamage(UntypedDamage d) => (1 - numericValue) * d;
    public TypedDamage ApplyToDamage(TypedDamage d) => (1 - numericValue) * d;

    public static Resistance operator +(Resistance left, Resistance right) =>
        new(Math.Clamp(left.numericValue + right.numericValue, 0, 1));
}
