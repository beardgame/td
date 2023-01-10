using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class DamageAttributor : Component, IListener<TakeDamage>, IListener<ObjectKilled>
{
    protected override void OnAdded()
    {
        Events.Subscribe<TakeDamage>(this);
        Events.Subscribe<ObjectKilled>(this);
    }

    public override void Update(TimeSpan elapsedTime) {}

    public void HandleEvent(TakeDamage @event)
    {
        @event.Source?.AttributeDamage(@event.Damage, Owner);
    }

    public void HandleEvent(ObjectKilled @event)
    {
        @event.LastDamageSource?.AttributeKill();
    }
}
