using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Resources;

readonly record struct Resource<T>(double Value)
    where T : IResourceType
{
    public static Resource<T> Zero => default;

    public static Resource<T> operator +(Resource<T> left, Resource<T> right) => new(left.Value + right.Value);
    public static Resource<T> operator -(Resource<T> left, Resource<T> right) => new(left.Value - right.Value);
    public static Resource<T> operator -(Resource<T> amount) => new(-amount.Value);
    public static double operator /(Resource<T> left, Resource<T> right) => left.Value / right.Value;

    public static Resource<T> operator *(double scalar, Resource<T> amount) => new(scalar * amount.Value);
    public static Resource<T> operator *(Resource<T> amount, double scalar) => new(scalar * amount.Value);
    public static Resource<T> operator /(Resource<T> amount, double divider) => new(amount.Value / divider);

    public static bool operator <(Resource<T> left, Resource<T> right) => left.Value < right.Value;
    public static bool operator >(Resource<T> left, Resource<T> right) => left.Value > right.Value;
    public static bool operator <=(Resource<T> left, Resource<T> right) => left.Value <= right.Value;
    public static bool operator >=(Resource<T> left, Resource<T> right) => left.Value >= right.Value;

    public static ResourcePerSecond<T> operator /(Resource<T> amount, TimeSpan seconds) => new(amount.Value / seconds.NumericValue);
}
