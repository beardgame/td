using Bearded.TD.Game.Simulation.Components;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons
{
    readonly struct ShotProjectile : IComponentEvent
    {
        public Position3 Position { get; }
        public Direction2 MuzzleDirection { get; }
        public Velocity3 Velocity { get; }

        public ShotProjectile(Position3 position, Direction2 muzzleDirection, Velocity3 velocity)
        {
            Position = position;
            MuzzleDirection = muzzleDirection;
            Velocity = velocity;
        }
    }
}
