using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using static Bearded.TD.Utilities.DebugAssert;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Damage;

interface IHealth
{
    HitPoints CurrentHealth { get; }
    HitPoints MaxHealth { get; }
    double HealthPercentage => CurrentHealth / MaxHealth;
}

[Component("health")]
sealed class Health :
    Component,
    IHealth,
    IPreviewListener<PreviewHealDamage>,
    IListener<HealDamage>
{
    public HitPoints CurrentHealth => pool?.CurrentHitPoints ?? throw new Exception();
    public HitPoints MaxHealth => pool?.MaxHitPoints ?? throw new Exception();

    private HitPointsPool? pool;

    protected override void OnAdded()
    {
        Events.Subscribe<PreviewHealDamage>(this);
        Events.Subscribe<HealDamage>(this);
        ComponentDependencies.Depend<HitPointsPool>(Owner, Events, p => pool = p, p => p.Shell == DamageShell.Health);
    }

    public override void OnRemoved()
    {
        State.IsInvalid("Can never remove health components.");
    }

    public void PreviewEvent(ref PreviewHealDamage @event)
    {
        @event = @event.CappedAt(MaxHealth - CurrentHealth);
    }

    public void HandleEvent(HealDamage @event)
    {
        pool?.RestoreHitPoints(@event.Heal.Heal.Amount);
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (CurrentHealth <= HitPoints.Zero)
        {
            Events.Send(new EnactDeath());
        }
    }
}
