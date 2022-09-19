using System;

namespace Bearded.TD.Game.Simulation.Navigation;

enum Passability : byte
{
    // Units that walk on floors only and cannot pass through obstacles.
    WalkingUnit = 1,
    // Units that do not need solid floor and can fly over obstacles.
    FlyingUnit = 2,
    // Player units that do not solid floor and can fly over obstacles.
    Drone = 3,
    // Projectiles.
    Projectile = 4,
    // Units that walk on floors and ignore obstacles.
    Bulldozer = 5
}

[Flags]
enum Passabilities : byte
{
    None = 0,

    // ReSharper disable once ShiftExpressionRealShiftCountIsZero
    WalkingUnit = 1 << (Passability.WalkingUnit - 1),
    FlyingUnit = 1 << (Passability.FlyingUnit - 1),
    Drone = 1 << (Passability.Drone - 1),
    Projectile = 1 << (Passability.Projectile - 1),
    Bulldozer = 1 << (Passability.Bulldozer - 1),

    All = 0xff
}

static class PassabilityExtensions
{
    public static Passabilities AsFlags(this Passability passability)
        => (Passabilities) (1 << ((byte) passability - 1));
}
