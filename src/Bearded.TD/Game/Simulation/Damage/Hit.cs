using Bearded.TD.Game.Simulation.Physics;

namespace Bearded.TD.Game.Simulation.Damage;

enum HitType
{
    Impact,
    AreaOfEffect,
    Self,
}

/// <summary>
/// Represents the concept of affecting a game object in a general way.
/// E.g. an impact from a projectile, or a tick of on-fire damage, or getting stunned, or getting healed, ...
/// </summary>
readonly record struct Hit(HitType Type, UntypedDamage DamagePotential, Impact? Impact)
{
    public static Hit FromImpact(Impact impact, UntypedDamage potential) => new(HitType.Impact, potential, impact);
    public static Hit FromAreaOfEffect(Impact impact, UntypedDamage potential)
        => new(HitType.AreaOfEffect, potential, impact);
    public static Hit FromSelf(UntypedDamage potential) => new(HitType.Self, potential, null);
}
