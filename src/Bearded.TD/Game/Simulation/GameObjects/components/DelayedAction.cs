using System;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class DelayedAction(Action action, TimeSpan delay) : Component
{
    private Instant? executionTime;

    protected override void OnAdded() {}

    public override void Activate()
    {
        executionTime = Owner.Game.Time + delay;
    }

    public override void Update(TimeSpan elapsedTime)
    {
        if (executionTime is not { } time || time < Owner.Game.Time)
        {
            return;
        }

        action();
        Owner.RemoveComponent(this);
    }
}

static class DelayedActionExtensions
{
    public static void Delay(this GameObject obj, Action action, TimeSpan delay)
    {
        obj.AddComponent(new DelayedAction(action, delay));
    }
}
