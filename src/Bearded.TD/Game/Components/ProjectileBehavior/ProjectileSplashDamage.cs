using System.Linq;
using Bearded.TD.Content.Models;
using Bearded.TD.Game.Damage;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Rendering;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Components.ProjectileBehavior
{
    [Component("splashDamageOnHit")]
    class ProjectileSplashDamage : Component<Projectile, IProjectileSplashDamageComponentParameters>
    {
        public ProjectileSplashDamage(IProjectileSplashDamageComponentParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
            Owner.HitEnemy += _ => onHit();
            Owner.HitLevel += onHit;
        }

        private void onHit()
        {
            var center = Owner.Position;
            var distanceSquared = Parameters.Range.Squared;

            var enemies = Owner.Game.UnitLayer;
            var tiles = Level.TilesInCircle(center.XY(), Parameters.Range);

            foreach (var enemy in tiles.SelectMany(enemies.GetUnitsOnTile))
            {
                if ((enemy.Position - center).LengthSquared > distanceSquared)
                    continue;

                enemy.Damage(new DamageInfo(Parameters.Damage, DamageType.Kinetic, Owner.DamageSource));
            }
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries) { }
    }
}
