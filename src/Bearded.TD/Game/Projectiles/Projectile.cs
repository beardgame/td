using amulware.Graphics;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.Utilities.Math;
using Bearded.Utilities.SpaceTime;
using OpenTK;

namespace Bearded.TD.Game.Projectiles
{
    class Projectile : GameObject
    {
        private readonly Building damageSource;
        private readonly int damage;

        public Position2 Position { get; private set; }
        public Velocity2 Velocity { get; private set; }
        public Tile<TileInfo> CurrentTile { get; private set; }

        public Projectile(Position2 position, Velocity2 velocity, int damage, Building damageSource)
        {
            this.damageSource = damageSource;
            this.damage = damage;
            Position = position;
            Velocity = velocity;
        }

        public Projectile(Position2 position, Direction2 direction, Speed speed, int damage, Building damageSource)
            : this(position, direction * speed, damage, damageSource)
        {
        }

        protected override void OnAdded()
        {
            CurrentTile = Game.Level.GetTile(Position);
        }

        public override void Update(TimeSpan elapsedTime)
        {
            var step = Velocity * elapsedTime;
            var ray = new Ray(Position, step);

            var tiles = new LevelRayCaster<TileInfo>().EnumerateTiles(Game.Level, ray);

            foreach (var tile in tiles)
            {
                if (!tile.IsValid || tile.Info.TileType == TileInfo.Type.Wall)
                {
                    Delete();
                    return;
                }

                var enemies = tile.Info.Enemies;

                (EnemyUnit Unit, float Factor) closestHit = (null, float.PositiveInfinity);

                foreach (var enemy in enemies)
                {
                    if (enemy.CollisionCircle.TryHit(ray, out var f, out var p, out var n))
                    {
                        if (f < closestHit.Factor)
                        {
                            closestHit = (enemy, f);
                        }
                    }
                }

                if (closestHit.Unit != null)
                {
                    closestHit.Unit.Damage(damage, damageSource);
                    Delete();
                    return;
                }

                CurrentTile = tile;
            }

            Position += step;
        }

        public override void Draw(GeometryManager geometries)
        {
            var geo = geometries.ConsoleBackground;
            geo.Color = Color.Yellow;
            
            geo.DrawRectangle(Position.NumericValue, Vector2.One * 0.1f);
        }
    }
}
