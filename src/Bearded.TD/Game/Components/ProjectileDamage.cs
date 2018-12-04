using Bearded.TD.Game.Projectiles;
using Bearded.TD.Game.Units;
using Bearded.TD.Mods.Models;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    [Component("damageOnHit")]
    class ProjectileDamage : IComponent<Projectile>
    {
        private readonly IProjectileDamageComponent parameters;
        public Projectile Owner { get; private set; }

        public ProjectileDamage(IProjectileDamageComponent parameters)
        {
            this.parameters = parameters;
        }
        
        public void OnAdded(Projectile owner)
        {
            Owner = owner;
            owner.HitEnemy += onHitEnemy;
        }

        private void onHitEnemy(object _, EnemyUnit enemy)
        {
            enemy.Damage(parameters.Damage, Owner.DamageSource);
        }

        public void Update(TimeSpan elapsedTime)
        {
        }

        public void Draw(GeometryManager geometries)
        {
        }
    }
}
