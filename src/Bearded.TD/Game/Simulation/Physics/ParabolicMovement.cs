using System;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Physics
{
    sealed class ParabolicMovement : Component<Projectile>
    {
        private Velocity3 velocity;
        private Tile tile;

        public ParabolicMovement(Velocity3 velocity)
        {
            this.velocity = velocity;
        }

        protected override void OnAdded()
        {
            tile = Level.GetTile(Owner.Position.XY());
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var forces = Constants.Game.Physics.Gravity3;

            var position = Owner.Position;
            velocity += forces * elapsedTime;

            var step = velocity * elapsedTime;
            var ray = new Ray(position.XY(), step.XY());

            var (result, rayFactor, _, enemy) = Owner.Game.Level.CastRayAgainstEnemies(
                ray, Owner.Game.UnitLayer, Owner.Game.PassabilityManager.GetLayer(Passability.Projectile));

            position += step * rayFactor;

            Owner.Position = position;

            tile = Level.GetTile(position.XY());

            switch (result)
            {
                case RayCastResultType.HitNothing:
                    if (position.Z < Owner.Game.GeometryLayer[tile].DrawInfo.Height)
                    {
                        Events.Send(new HitLevel());
                        Owner.Delete();
                    }
                    break;
                case RayCastResultType.HitLevel:
                    Events.Send(new HitLevel());
                    Owner.Delete();
                    break;
                case RayCastResultType.HitEnemy:
                    enemy.Match(e => Events.Send(new HitEnemy(e)), () => throw new InvalidOperationException());
                    Owner.Delete();
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public override void Draw(CoreDrawers drawers)
        {
        }
    }
}
