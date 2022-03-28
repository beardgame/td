using Bearded.TD.Game.Commands;
using Bearded.TD.Game.Commands.Gameplay;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Units;

sealed class EnemyLifeCycle : Component,
    IDamageTarget,
    IEnemyLife,
    IListener<EnactDeath>,
    IListener<TakeDamage>
{
    private bool isDead;
    private IDamageSource? lastDamageSource;

    protected override void OnAdded()
    {
        Events.Subscribe<EnactDeath>(this);
        Events.Subscribe<TakeDamage>(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (isDead)
        {
            Owner.Sync(KillUnit.Command, Owner, lastDamageSource);
        }
    }

    public void HandleEvent(TakeDamage @event)
    {
        lastDamageSource = @event.Source ?? lastDamageSource;
    }

    public void HandleEvent(EnactDeath @event)
    {
        isDead = true;
    }

    public void Kill(IDamageSource? damageSource)
    {
        Owner.Game.Meta.Events.Send(new EnemyKilled(Owner, damageSource));
        damageSource?.AttributeKill(this);
        Owner.Delete();
    }
}

interface IEnemyLife
{
    void Kill(IDamageSource? damageSource);
}
