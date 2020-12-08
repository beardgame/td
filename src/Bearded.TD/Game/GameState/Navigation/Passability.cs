using System;

namespace Bearded.TD.Game.GameState.Navigation
{
    enum Passability : byte
    {
        WalkingUnit = 1,
        FlyingUnit = 2,
        Worker = 3,
        Projectile = 4
    }

    [Flags]
    enum Passabilities : byte
    {
        None = 0,

        // ReSharper disable once ShiftExpressionRealShiftCountIsZero
        WalkingUnit = 1 << (Passability.WalkingUnit - 1),
        FlyingUnit = 1 << (Passability.FlyingUnit - 1),
        Worker = 1 << (Passability.Worker - 1),
        Projectile = 1 << (Passability.Projectile - 1),

        All = 0xff
    }

    static class PassabilityExtensions
    {
        public static Passabilities AsFlags(this Passability passability)
            => (Passabilities) (1 << ((byte) passability - 1));
    }
}
