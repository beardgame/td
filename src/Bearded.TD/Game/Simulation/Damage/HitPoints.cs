using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct HitPoints : IDiscreteMeasure1
{
    public static HitPoints Zero { get; } = new(0);

    public static HitPoints Max { get; } = new(int.MaxValue);

    public int NumericValue { get; }

    public HitPoints(int numericValue)
    {
        NumericValue = numericValue;
    }

    public bool Equals(HitPoints other) => NumericValue.Equals(other.NumericValue);

    public override bool Equals(object? obj) => obj is HitPoints other && Equals(other);

    public override int GetHashCode() => NumericValue.GetHashCode();

    public override string ToString() => $"{NumericValue} hit points";

    public static bool operator ==(HitPoints left, HitPoints right) => left.Equals(right);

    public static bool operator !=(HitPoints left, HitPoints right) => !left.Equals(right);

    public static bool operator <(HitPoints left, HitPoints right) =>
        left.NumericValue < right.NumericValue;

    public static bool operator <=(HitPoints left, HitPoints right) =>
        left.NumericValue <= right.NumericValue;

    public static bool operator >(HitPoints left, HitPoints right) =>
        left.NumericValue > right.NumericValue;

    public static bool operator >=(HitPoints left, HitPoints right) =>
        left.NumericValue >= right.NumericValue;

    public static HitPoints operator +(HitPoints left, HitPoints right) =>
        new(left.NumericValue + right.NumericValue);

    public static HitPoints operator -(HitPoints left, HitPoints right) =>
        new(left.NumericValue - right.NumericValue);

    public static HitPoints operator -(HitPoints amount) => new(-amount.NumericValue);

    public static HitPoints operator *(int scalar, HitPoints amount) =>
        new(scalar * amount.NumericValue);

    public static HitPoints operator *(HitPoints amount, int scalar) =>
        new(scalar * amount.NumericValue);

    public static double operator /(HitPoints left, HitPoints right) =>
        (double) left.NumericValue / right.NumericValue;
}

static class HitPointsExtensions
{
    public static HitPoints HitPoints(this int amount) => new(amount);
}