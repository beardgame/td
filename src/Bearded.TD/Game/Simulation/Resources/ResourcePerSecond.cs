using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct ResourcePerSecond<T>(double Value)
    where T : IResourceType
{
    public static Resource<T> Zero => default;

    public static ResourcePerSecond<T> operator +(ResourcePerSecond<T> left, ResourcePerSecond<T> right) => new(left.Value + right.Value);
    public static ResourcePerSecond<T> operator -(ResourcePerSecond<T> left, ResourcePerSecond<T> right) => new(left.Value - right.Value);
    public static ResourcePerSecond<T> operator -(ResourcePerSecond<T> amount) => new(-amount.Value);
    public static double operator /(ResourcePerSecond<T> left, ResourcePerSecond<T> right) => left.Value / right.Value;

    public static ResourcePerSecond<T> operator *(double scalar, ResourcePerSecond<T> amount) => new(scalar * amount.Value);
    public static ResourcePerSecond<T> operator *(ResourcePerSecond<T> amount, double scalar) => new(scalar * amount.Value);
    public static ResourcePerSecond<T> operator /(ResourcePerSecond<T> amount, double divider) => new(amount.Value / divider);

    public static bool operator <(ResourcePerSecond<T> left, ResourcePerSecond<T> right) => left.Value < right.Value;
    public static bool operator >(ResourcePerSecond<T> left, ResourcePerSecond<T> right) => left.Value > right.Value;
    public static bool operator <=(ResourcePerSecond<T> left, ResourcePerSecond<T> right) => left.Value <= right.Value;
    public static bool operator >=(ResourcePerSecond<T> left, ResourcePerSecond<T> right) => left.Value >= right.Value;

    public static Resource<T> operator *(ResourcePerSecond<T> amount, TimeSpan seconds) => new(amount.Value * seconds.NumericValue);
    public static Resource<T> operator *(TimeSpan seconds, ResourcePerSecond<T> amount) => new(amount.Value * seconds.NumericValue);
}
