using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Projectiles;
using Bearded.TD.Rendering;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components.Damage
{
    [Component("damageOnHit")]
    sealed class ProjectileDamage : Component<Projectile, IProjectileDamageComponentParameters>, IListener<HitEnemy>
    {
        public ProjectileDamage(IProjectileDamageComponentParameters parameters) : base(parameters) {}

        protected override void Initialize()
        {
            Events.Subscribe(this);
        }

        public void HandleEvent(HitEnemy @event)
        {
            if (!@event.Enemy.TryGetSingleComponent<IDamageExecutor>(out var damageExecutor))
            {
                DebugAssert.State.IsInvalid();
            }

            var result = damageExecutor.Damage(
                new DamageInfo(Parameters.Damage, Parameters.Type ?? DamageType.Kinetic, Owner.DamageSource));
            Events.Send(new CausedDamage(@event.Enemy, result));
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers) { }
    }
}
