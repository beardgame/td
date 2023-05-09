using System;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class TriggerSubscription<T> : ITriggerSubscription, IListener<T> where T : struct, IComponentEvent
{
    private readonly Action action;

    public TriggerSubscription(Action action)
    {
        this.action = action;
    }

    public void HandleEvent(T @event)
    {
        action();
    }

    public void Unsubscribe(ComponentEvents events)
    {
        events.Unsubscribe(this);
    }
}
