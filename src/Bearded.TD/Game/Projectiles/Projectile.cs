using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Projectiles
{
    [ComponentOwner]
    class Projectile : GameObject
    {
        private readonly ProjectileBlueprint blueprint;
        private readonly Building damageSource;
        private readonly ComponentCollection<Projectile> components = new ComponentCollection<Projectile>();

        public Position2 Position { get; private set; }
        public Velocity2 Velocity { get; private set; }
        public Tile<TileInfo> CurrentTile { get; private set; }
        
        public Projectile(ProjectileBlueprint blueprint, Position2 position, Velocity2 velocity, Building damageSource)
        {
            this.blueprint = blueprint;
            this.damageSource = damageSource;
            Position = position;
            Velocity = velocity;
        }

        public Projectile(ProjectileBlueprint blueprint, Position2 position, Direction2 direction, Speed speed, Building damageSource)
            : this(blueprint, position, direction * speed, damageSource)
        {
        }

        protected override void OnAdded()
        {
            CurrentTile = Game.Level.GetTile(Position);

            components.Add(this, blueprint.GetComponents());
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var step = Velocity * elapsedTime;
            var ray = new Ray(Position, step);
            
            foreach (var tile in Game.Level.Cast(ray))
            {
                if (!tile.IsValid || !tile.Info.IsPassableFor(TileInfo.PassabilityLayer.Projectile))
                {
                    Delete();
                    return;
                }

                var enemies = tile.Info.Enemies;

                (EnemyUnit Unit, float Factor) closestHit = (null, float.PositiveInfinity);

                foreach (var enemy in enemies)
                {
                    if (enemy.CollisionCircle.TryHit(ray, out var f, out _, out _)
                        && f < closestHit.Factor)
                    {
                        closestHit = (enemy, f);
                    }
                }

                if (closestHit.Unit != null)
                {
                    closestHit.Unit.Damage(blueprint.Damage, damageSource);
                    Delete();
                    return;
                }

                CurrentTile = tile;
            }

            Position += step;

            components.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = blueprint.Color;
            
            geo.DrawRectangle(Position.NumericValue, Vector2.One * 0.1f);

            components.Draw(geometries);
        }
    }
}
