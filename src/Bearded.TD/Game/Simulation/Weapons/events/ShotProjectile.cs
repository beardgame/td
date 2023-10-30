using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

[Trigger("shotProjectile")]
readonly record struct ShotProjectile(
        Position3 Position,
        Direction2 MuzzleDirection,
        Velocity3 Velocity,
        GameObject Projectile)
    : IComponentEvent;
