using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Game.Simulation.Damage;

readonly struct HealInfo
{
    public HitPoints Amount { get; }

    public HealInfo(HitPoints amount)
    {
        Argument.Satisfies(amount >= HitPoints.Zero);
        Amount = amount;
    }

    public HealInfo WithAdjustedAmount(HitPoints newAmount) => new(newAmount);
}