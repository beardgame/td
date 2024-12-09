namespace Bearded.TD.Game.Simulation.Core;

readonly record struct Corruption(double Value)
{
    public static Corruption Zero => default;

    public static Corruption operator +(Corruption left, Corruption right) => new(left.Value + right.Value);
    public static Corruption operator -(Corruption left, Corruption right) => new(left.Value - right.Value);
    public static Corruption operator -(Corruption amount) => new(-amount.Value);

    public static Corruption operator *(double scalar, Corruption amount) => new(scalar * amount.Value);
    public static Corruption operator *(Corruption amount, double scalar) => new(scalar * amount.Value);
    public static Corruption operator /(Corruption amount, double divider) => new(amount.Value / divider);
}
