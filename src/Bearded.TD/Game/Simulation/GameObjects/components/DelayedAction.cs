using System;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

[Flags]
enum DelayMode
{
    OnTimeOut = 1,
    OnDeleting = 2,
    OnTimeOutOrDeleting = OnTimeOut | OnDeleting,
}

sealed class DelayedAction(Action action, TimeSpan? delay, DelayMode mode)
    : Component, IListener<ObjectDeleting>
{
    private Instant? executionTime;

    protected override void OnAdded() {}

    public override void Activate()
    {
        if (mode.HasFlag(DelayMode.OnTimeOut))
            executionTime = Owner.Game.Time + (delay ?? TimeSpan.Zero);

        if (mode.HasFlag(DelayMode.OnDeleting))
            Events.Subscribe(this);
    }

    public void HandleEvent(ObjectDeleting e)
    {
        action();
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (executionTime is not { } time || time >= Owner.Game.Time)
        {
            return;
        }

        action();
        Owner.RemoveComponent(this);
    }

}

static class DelayedActionExtensions
{
    public static void Delay(
        this GameObject obj, Action action, TimeSpan? delay = null, DelayMode mode = DelayMode.OnTimeOut)
    {
        obj.AddComponent(new DelayedAction(action, delay, mode));
    }
}
