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
    sealed class ProjectileDamage : Component<IComponentOwner, IProjectileDamageComponentParameters>, IListener<HitEnemy>
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
                return;
            }

            Owner.TryGetSingleComponentInOwnerTree<IDamageSource>(out var damageSource);

            var result = damageReceiver.Damage(
                new DamageInfo(Parameters.Damage, Parameters.Type ?? DamageType.Kinetic), damageSource);
            Events.Send(new CausedDamage(result));
        }

        public override void Update(TimeSpan elapsedTime) { }

        public override void Draw(CoreDrawers drawers) { }
    }
}
