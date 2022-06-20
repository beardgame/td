using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct UntypedDamage
{
    public HitPoints Amount { get; }

    public UntypedDamage(HitPoints amount)
    {
        Argument.Satisfies(amount > HitPoints.Zero);
        Amount = amount;
    }

    public TypedDamage Typed(DamageType type) => new(Amount, type);
}
