using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct TypedDamage
{
    public HitPoints Amount { get; }
    public DamageType Type { get; }

    public TypedDamage(HitPoints amount, DamageType type)
    {
        Argument.Satisfies(amount >= HitPoints.Zero);
        Amount = amount;
        Type = type;
    }

    public TypedDamage WithAdjustedAmount(HitPoints newAmount) => new(newAmount, Type);
}
