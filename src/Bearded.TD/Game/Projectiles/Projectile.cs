using System;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Navigation;
using Bearded.TD.Game.Units;
using Bearded.TD.Game.World;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Projectiles
{
    [ComponentOwner]
    class Projectile : GameObject
    {
        public event EventHandler<EnemyUnit> HitEnemy;
        public Building DamageSource { get; }
        
        private readonly IProjectileBlueprint blueprint;
        private readonly ComponentCollection<Projectile> components = new ComponentCollection<Projectile>();

        public Position2 Position { get; private set; }
        public Velocity2 Velocity { get; private set; }
        public Tile CurrentTile { get; private set; }
        
        public Projectile(IProjectileBlueprint blueprint, Position2 position, Velocity2 velocity, Building damageSource)
        {
            this.blueprint = blueprint;
            DamageSource = damageSource;
            Position = position;
            Velocity = velocity;
        }

        public Projectile(IProjectileBlueprint blueprint, Position2 position, Direction2 direction, Speed speed, Building damageSource)
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

            var (result, _, _, enemy) = Game.Level.CastRayAgainstEnemies(
                ray, Game.UnitLayer, Game.PassabilityManager.GetLayer(Passability.Projectile));
            
            switch (result)
            {
                case RayCastResult.HitNothing:
                    break;
                case RayCastResult.HitLevel:
                    Delete();
                    break;
                case RayCastResult.HitEnemy:
                    HitEnemy?.Invoke(this, enemy);
                    Delete();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (Deleted)
                return;

            Position += step;

            components.Update(elapsedTime);
        }

        public override void Draw(GeometryManager geometries)
        {
            blueprint.Sprite.Draw(Position.NumericValue.WithZ(0), blueprint.Color, 0.1f);

            components.Draw(geometries);
        }
    }
}
