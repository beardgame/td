using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    [Component("damageOnHit")]
    sealed class DamageOnHit : Component<IComponentOwner, IDamageOnHitComponentParameters>, IListener<HitEnemy>
    {
        public DamageOnHit(IDamageOnHitComponentParameters parameters) : base(parameters) {}

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
            var damage = new DamageInfo(Parameters.Damage, Parameters.Type ?? DamageType.Kinetic);
            var damageDone = DamageExecutor.FromObject(Owner).TryDoDamage(@event.Enemy, damage);
            DebugAssert.State.Satisfies(damageDone);
        }

        public override void Update(TimeSpan elapsedTime) { }
    }
}
