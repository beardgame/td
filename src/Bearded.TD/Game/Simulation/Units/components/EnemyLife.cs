using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

sealed class EnemyLife : Component, IListener<ObjectKilled>
{
    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public void HandleEvent(ObjectKilled @event)
    {
        Owner.Game.Meta.Events.Send(new EnemyKilled(Owner, @event.LastDamageSource));
    }

    public override void Update(TimeSpan elapsedTime) {}
}
