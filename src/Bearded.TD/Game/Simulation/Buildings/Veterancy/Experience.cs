using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Buildings.Veterancy;

readonly record struct Experience(float NumericValue) : IMeasure1F
{
    public static Experience Zero { get; } = new(0);

    public override string ToString() => $"{NumericValue} xp";

    public static bool operator <(Experience left, Experience right) =>
        left.NumericValue < right.NumericValue;

    public static bool operator <=(Experience left, Experience right) =>
        left.NumericValue <= right.NumericValue;

    public static bool operator >(Experience left, Experience right) =>
        left.NumericValue > right.NumericValue;

    public static bool operator >=(Experience left, Experience right) =>
        left.NumericValue >= right.NumericValue;

    public static Experience operator +(Experience left, Experience right) =>
        new(left.NumericValue + right.NumericValue);

    public static Experience operator -(Experience left, Experience right) =>
        new(left.NumericValue - right.NumericValue);

    public static Experience operator *(float scalar, Experience amount) =>
        new(scalar * amount.NumericValue);

    public static Experience operator *(Experience amount, float scalar) =>
        new(scalar * amount.NumericValue);

    public static Experience operator /(Experience amount, float divider) =>
        new(amount.NumericValue / divider);

    public static double operator /(Experience left, Experience right) =>
        (double) left.NumericValue / right.NumericValue;
}

static class ExperienceExtensions
{
    public static Experience Xp(this int amount) => new(amount);
    public static Experience Xp(this float amount) => new(amount);
}
