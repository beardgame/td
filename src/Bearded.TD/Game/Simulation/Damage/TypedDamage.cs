using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct TypedDamage
{
    public HitPoints Amount { get; }
    public DamageType Type { get; }

    public static TypedDamage Zero(DamageType type) => new(HitPoints.Zero, type);

    public TypedDamage(HitPoints amount, DamageType type)
    {
        Argument.Satisfies(amount >= HitPoints.Zero);
        Amount = amount;
        Type = type;
    }

    public UntypedDamage Untyped() => new(Amount);

    public TypedDamage WithAdjustedAmount(HitPoints newAmount) => new(newAmount, Type);

    public static TypedDamage operator *(int scalar, TypedDamage damage) =>
        damage.WithAdjustedAmount(scalar * damage.Amount);

    public static TypedDamage operator *(float scalar, TypedDamage damage) =>
        damage.WithAdjustedAmount((scalar * damage.Amount.NumericValue).HitPoints());

    public static TypedDamage operator *(TypedDamage damage, int scalar) =>
        damage.WithAdjustedAmount(scalar * damage.Amount);

    public static TypedDamage operator *(TypedDamage damage, float scalar) =>
        damage.WithAdjustedAmount((scalar * damage.Amount.NumericValue).HitPoints());

    public static TypedDamage operator /(TypedDamage damage, int scalar) =>
        damage.WithAdjustedAmount((damage.Amount.NumericValue / scalar).HitPoints());

    public static TypedDamage operator /(TypedDamage damage, float scalar) =>
        damage.WithAdjustedAmount((damage.Amount.NumericValue / scalar).HitPoints());
}
