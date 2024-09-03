namespace Bearded.TD.Game.Simulation.Resources;

static class ExchangeRate
{
    public static ExchangeRate<TFrom, TTo> FromTo<TFrom, TTo>(Resource<TFrom> from, Resource<TTo> to)
        where TFrom : IResourceType
        where TTo : IResourceType
        => new(to.Value / from.Value);
}

readonly record struct ExchangeRate<TFrom, TTo>(double Value)
    where TFrom : IResourceType
    where TTo : IResourceType
{
    public static ExchangeRate<TFrom, TTo> operator +(ExchangeRate<TFrom, TTo> left, ExchangeRate<TFrom, TTo> right) => new(left.Value + right.Value);
    public static ExchangeRate<TFrom, TTo> operator -(ExchangeRate<TFrom, TTo> left, ExchangeRate<TFrom, TTo> right) => new(left.Value - right.Value);
    public static ExchangeRate<TFrom, TTo> operator -(ExchangeRate<TFrom, TTo> rate) => new(-rate.Value);
    public static double operator /(ExchangeRate<TFrom, TTo> left, ExchangeRate<TFrom, TTo> right) => left.Value / right.Value;

    public static ExchangeRate<TFrom, TTo> operator *(double scalar, ExchangeRate<TFrom, TTo> rate) => new(scalar * rate.Value);
    public static ExchangeRate<TFrom, TTo> operator *(ExchangeRate<TFrom, TTo> rate, double scalar) => new(scalar * rate.Value);
    public static ExchangeRate<TFrom, TTo> operator /(ExchangeRate<TFrom, TTo> rate, double divider) => new(rate.Value / divider);

    public static bool operator <(ExchangeRate<TFrom, TTo> left, ExchangeRate<TFrom, TTo> right) => left.Value < right.Value;
    public static bool operator >(ExchangeRate<TFrom, TTo> left, ExchangeRate<TFrom, TTo> right) => left.Value > right.Value;
    public static bool operator <=(ExchangeRate<TFrom, TTo> left, ExchangeRate<TFrom, TTo> right) => left.Value <= right.Value;
    public static bool operator >=(ExchangeRate<TFrom, TTo> left, ExchangeRate<TFrom, TTo> right) => left.Value >= right.Value;

    public static Resource<TTo> operator *(Resource<TFrom> amount, ExchangeRate<TFrom, TTo> rate) => new(amount.Value * rate.Value);
    public static Resource<TTo> operator *(ExchangeRate<TFrom, TTo> rate, Resource<TFrom> amount) => new(amount.Value * rate.Value);
    public static Resource<TFrom> operator /(Resource<TTo> amount, ExchangeRate<TFrom, TTo> rate) => new(amount.Value / rate.Value);
}
