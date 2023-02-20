using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

readonly record struct HitPoints(float NumericValue) : IMeasure1F
{
    public static HitPoints Zero { get; } = new(0);

    public static HitPoints Max { get; } = new(int.MaxValue);

    public override string ToString() => $"{NumericValue} hit points";

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

    public static HitPoints operator *(float scalar, HitPoints amount) =>
        new(scalar * amount.NumericValue);

    public static HitPoints operator *(HitPoints amount, float scalar) =>
        new(scalar * amount.NumericValue);

    public static double operator /(HitPoints left, HitPoints right) =>
        (double) left.NumericValue / right.NumericValue;

    public HitPoints Discrete() => new((int) NumericValue);
}

static class HitPointsExtensions
{
    public static HitPoints HitPoints(this int amount) => new(amount);
    public static HitPoints HitPoints(this float amount) => new(amount);
}
