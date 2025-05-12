using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

sealed class Healable :
    Component,
    IPreviewListener<PreviewHealDamage>,
    IListener<HealDamage>
{
    private HitPoints currentHealth => pool?.CurrentHitPoints ?? throw new Exception();
    private HitPoints maxHealth => pool?.MaxHitPoints ?? throw new Exception();

    private ComponentDependencies.IDependencyRef? poolDependency;
    private HitPointsPool? pool;

    protected override void OnAdded()
    {
        Events.Subscribe<PreviewHealDamage>(this);
        Events.Subscribe<HealDamage>(this);
        poolDependency = ComponentDependencies.Depend<HitPointsPool>(
            Owner, Events, p => pool = p, p => p.Shell == DamageShell.Health);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe<PreviewHealDamage>(this);
        Events.Unsubscribe<HealDamage>(this);
        poolDependency?.Dispose();
        poolDependency = null;
    }

    public void PreviewEvent(ref PreviewHealDamage @event)
    {
        @event = @event.CappedAt(maxHealth - currentHealth);
    }

    public void HandleEvent(HealDamage @event)
    {
        pool?.RestoreHitPoints(@event.Heal.Heal.Amount);
    }

    public override void Update(TimeSpan elapsedTime) {}
}
