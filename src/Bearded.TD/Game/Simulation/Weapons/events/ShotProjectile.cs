using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Trigger("shotProjectile")]
readonly record struct ShotProjectile(
        Position3 Position,
        Direction2 MuzzleDirection,
        Velocity3 Velocity,
        GameObject Projectile,
        UntypedDamage Damage)
    : IComponentEvent;

[Trigger("shotProjectiles")]
readonly record struct ShotProjectiles(
    Position3 Position,
    Direction2 MuzzleDirection,
    int Count,
    UntypedDamage Damage)
    : IComponentEvent;
