using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Rendering;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    sealed class DamageSource : Component<Projectile>, IListener<CausedDamage>
    {
        public readonly IDamageSource Source;

        public DamageSource(IDamageSource source)
        {
            Source = source;
        }

        protected override void OnAdded()
        {
            Events.Subscribe(this);
        }

        public override void Update(TimeSpan elapsedTime)
        {
        }

        public override void Draw(CoreDrawers drawers)
        {
        }

        public void HandleEvent(CausedDamage @event)
        {
            Source.AttributeDamage(@event.Target, @event.Result);
        }
    }
}
