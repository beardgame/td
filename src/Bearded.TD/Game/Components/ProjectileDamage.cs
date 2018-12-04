using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    [Component("damageOnHit")]
    class ProjectileDamage : Component<Projectile, IProjectileDamageComponent>
    {
        public ProjectileDamage(IProjectileDamageComponent parameters) : base(parameters)
        {
        }
        
        protected override void Initialise()
        {
            Owner.HitEnemy += onHitEnemy;
        }
        
        private void onHitEnemy(EnemyUnit enemy)
        {
            enemy.Damage(Parameters.Damage, Owner.DamageSource);
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries) { }
    }
}
