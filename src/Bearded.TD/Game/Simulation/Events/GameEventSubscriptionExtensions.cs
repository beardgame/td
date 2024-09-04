using System;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Events;

public delegate void EventHandler<in T>(T e) where T : struct, IEvent;
public delegate void PreviewEventHandler<T>(ref T e) where T : struct, IPreviewEvent;

static class GameEventSubscriptionExtensions
{
    public static IDisposable Subscribe<TEvent>(
        this ComponentEvents events,
        EventHandler<TEvent> handle)
        where TEvent : struct, IComponentEvent
    {
        var subscription = new GameEventsSubscription<IComponentEvent, IComponentPreviewEvent>.DisposableSubscription<TEvent>(events, handle);
        events.Subscribe(subscription);
        return subscription;
    }

    public static IDisposable Subscribe<TEvent>(
        this GameEvents<IComponentEvent, IComponentPreviewEvent> events,
        PreviewEventHandler<TEvent> handle)
        where TEvent : struct, IComponentPreviewEvent
    {
        var subscription = new GameEventsSubscription<IComponentEvent, IComponentPreviewEvent>.DisposablePreviewSubscription<TEvent>(events, handle);
        events.Subscribe(subscription);
        return subscription;
    }

    public static IDisposable Subscribe<TEvent>(
        this GameEvents<IGlobalEvent, IGlobalPreviewEvent> events,
        EventHandler<TEvent> handle)
        where TEvent : struct, IGlobalEvent
    {
        var subscription = new GameEventsSubscription<IGlobalEvent, IGlobalPreviewEvent>.DisposableSubscription<TEvent>(events, handle);
        events.Subscribe(subscription);
        return subscription;
    }

    public static IDisposable Subscribe<TEvent>(
        this GameEvents<IGlobalEvent, IGlobalPreviewEvent> events,
        PreviewEventHandler<TEvent> handle)
        where TEvent : struct, IGlobalPreviewEvent
    {
        var subscription = new GameEventsSubscription<IGlobalEvent, IGlobalPreviewEvent>.DisposablePreviewSubscription<TEvent>(events, handle);
        events.Subscribe(subscription);
        return subscription;
    }
}

static class GameEventsSubscription<TEventInterface, TPreviewEventInterface>
    where TEventInterface : IEvent
    where TPreviewEventInterface : IPreviewEvent
{
    public sealed class DisposableSubscription<TEvent>(
        GameEvents<TEventInterface, TPreviewEventInterface> events,
        EventHandler<TEvent> handle)
        : IListener<TEvent>, IDisposable
        where TEvent : struct, TEventInterface
    {
        public void Dispose()
        {
            events.Unsubscribe(this);
        }

        public void HandleEvent(TEvent e)
        {
            handle(e);
        }
    }

    public sealed class DisposablePreviewSubscription<TEvent>(
        GameEvents<TEventInterface, TPreviewEventInterface> events,
        PreviewEventHandler<TEvent> handle)
        : IPreviewListener<TEvent>, IDisposable
        where TEvent : struct, TPreviewEventInterface
    {
        public void Dispose()
        {
            events.Unsubscribe(this);
        }

        public void PreviewEvent(ref TEvent e)
        {
            handle(ref e);
        }
    }
}
