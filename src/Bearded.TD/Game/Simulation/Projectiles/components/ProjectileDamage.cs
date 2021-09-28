using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    [Component("damageOnHit")]
    sealed class ProjectileDamage : Component<Projectile, IProjectileDamageComponentParameters>, IListener<HitEnemy>
    {
        public ProjectileDamage(IProjectileDamageComponentParameters parameters) : base(parameters) {}

        protected override void OnAdded()
        {
            Events.Subscribe(this);
        }

        public override void OnRemoved()
        {
            Events.Unsubscribe(this);
        }

        public void HandleEvent(HitEnemy @event)
        {
            if (!@event.Enemy.TryGetSingleComponent<IDamageReceiver>(out var damageReceiver))
            {
                DebugAssert.State.IsInvalid();
            }

            var result = damageReceiver.Damage(
                new DamageInfo(Parameters.Damage, Parameters.Type ?? DamageType.Kinetic, Owner.DamageSource));
            Events.Send(new CausedDamage(@event.Enemy, result));
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers) { }
    }
}
