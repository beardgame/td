using System;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.GameObjects;

interface ITrigger
{
    ISubscription Subscribe(ComponentEvents events, Action action);
}

sealed class Trigger<T> : ITrigger where T : struct, IComponentEvent
{
    public ISubscription Subscribe(ComponentEvents events, Action action)
    {
        var sub = new Subscription<T>(action);
        events.Subscribe(sub);
        return sub;
    }
}

interface ISubscription
{
    void Unsubscribe(ComponentEvents events);
}

sealed class Subscription<T> : ISubscription, IListener<T> where T : struct, IComponentEvent
{
    private readonly Action action;

    public Subscription(Action action)
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
