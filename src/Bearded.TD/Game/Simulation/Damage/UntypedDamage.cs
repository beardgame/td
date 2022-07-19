using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct UntypedDamage
{
    public HitPoints Amount { get; }

    public static UntypedDamage Zero => new(HitPoints.Zero);

    public UntypedDamage(HitPoints amount)
    {
        Argument.Satisfies(amount >= HitPoints.Zero);
        Amount = amount;
    }

    public TypedDamage Typed(DamageType type) => new(Amount, type);

    public static UntypedDamage operator *(int scalar, UntypedDamage amount) =>
        new(scalar * amount.Amount);

    public static UntypedDamage operator *(UntypedDamage amount, int scalar) =>
        new(scalar * amount.Amount);
}
