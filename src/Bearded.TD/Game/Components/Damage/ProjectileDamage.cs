using Bearded.TD.Content.Models;
using Bearded.TD.Game.Components.Events;
using Bearded.TD.Game.Damage;
using Bearded.TD.Game.Events;
using Bearded.TD.Game.Projectiles;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components.Damage
{
    [Component("damageOnHit")]
    class ProjectileDamage : Component<Projectile, IProjectileDamageComponentParameters>, IListener<HitEnemy>
    {
        public ProjectileDamage(IProjectileDamageComponentParameters parameters) : base(parameters)
        {
        }

        protected override void Initialise()
        {
            Events.Subscribe(this);
        }

        public void HandleEvent(HitEnemy @event)
        {
            @event.Enemy.Damage(new DamageInfo(Parameters.Damage, Parameters.Type ?? DamageType.Kinetic, Owner.DamageSource));
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(GeometryManager geometries) { }
    }
}
