using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Projectiles;

[Component("damageOnHit")]
sealed class DamageOnHit : Component, IListener<HitEnemy>
{
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
        var damageDone = Owner.TryGetProperty<DamageInfo>(out var damage)
            && DamageExecutor.FromObject(Owner).TryDoDamage(@event.Enemy, damage);
        DebugAssert.State.Satisfies(damageDone);
    }

    public override void Update(TimeSpan elapsedTime) { }
}
