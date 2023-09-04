using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class Killable : Component, IKillable, IListener<TookDamage>, IListener<EnactDeath>
{
    private IDamageSource? lastDamageSource;
    private bool isDead;

    protected override void OnAdded()
    {
        Events.Subscribe<TookDamage>(this);
        Events.Subscribe<EnactDeath>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (isDead)
        {
            Owner.Sync(KillGameObject.Command, Owner, lastDamageSource);
        }
    }

    public void HandleEvent(TookDamage @event)
    {
        if (!isDead)
        {
            lastDamageSource = @event.Source ?? lastDamageSource;
        }
    }

    public void HandleEvent(EnactDeath @event)
    {
        isDead = true;
    }

    // Expected to be called from synchronized code.
    public void Kill(IDamageSource? damageSource)
    {
        Events.Send(new ObjectKilled(damageSource ?? lastDamageSource));
        Owner.Delete();
    }
}

interface IKillable
{
    void Kill(IDamageSource? damageSource);
}
