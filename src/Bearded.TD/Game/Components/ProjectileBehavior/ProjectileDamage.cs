using Bearded.TD.Content.Models;
using Bearded.TD.Game.Damage;
using Bearded.TD.Game.Elements;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.ProjectileBehavior
{
    [Component("damageOnHit")]
    class ProjectileDamage : Component<Projectile, IProjectileDamageComponentParameters>
    {
        private DamageType damageType;

        public ProjectileDamage(IProjectileDamageComponentParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
            Owner.HitEnemy += onHitEnemy;
            damageType = Parameters.Type ?? DamageType.Kinetic;
        }

        private void onHitEnemy(EnemyUnit enemy)
        {
            enemy.Damage(new DamageInfo(Parameters.Damage, damageType, Owner.DamageSource));
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries) { }
    }
}
