using System;
using Bearded.TD.Shared.Events;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.GameObjects;

sealed class EventListener<TEvent> : Component, IListener<TEvent>
    where TEvent : struct, IComponentEvent
{
    private readonly Action<TEvent> handler;

    public EventListener(Action<TEvent> handler)
    {
        this.handler = handler;
    }

    protected override void OnAdded()
    {
        Events.Subscribe(this);
    }

    public override void OnRemoved()
    {
        Events.Unsubscribe(this);
    }

    public override void Update(TimeSpan elapsedTime)
    {
    }

    public void HandleEvent(TEvent e)
    {
        handler(e);
    }
}
