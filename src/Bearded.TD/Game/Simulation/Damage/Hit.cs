using Bearded.TD.Game.Simulation.Physics;
using Bearded.TD.Game.Simulation.Projectiles;

namespace Bearded.TD.Game.Simulation.Damage;

enum HitType
{
    Impact,
    AreaOfEffect,
    Self,
}

readonly record struct Hit(HitType Type, Impact? Impact)
{
    public static Hit FromImpact(Impact impact) => new(HitType.Impact, impact);
    public static Hit FromAreaOfEffect(Impact impact) => new(HitType.AreaOfEffect, impact);
    public static Hit FromSelf() => new(HitType.Self, null);
}
