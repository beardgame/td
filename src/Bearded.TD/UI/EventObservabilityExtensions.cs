using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using JetBrains.Annotations;

namespace Bearded.TD.UI;

static class EventObservabilityExtensions
{
    public static IObservable<T> Observe<T>(this GlobalGameEvents events)
        where T : struct, IGlobalEvent
    {
        return new Observable<IGlobalEvent, IGlobalPreviewEvent, T>(events);
    }

    public static IObservable<T> Observe<T>(this ComponentEvents events)
        where T : struct, IComponentEvent
    {
        return new Observable<IComponentEvent, IComponentPreviewEvent, T>(events);
    }

    private sealed class Observable<TEventInterface, TPreviewEventInterface, T>(
        GameEvents<TEventInterface, TPreviewEventInterface> events
    )
        : IListener<T>, IObservable<T>
        where TEventInterface : IEvent
        where TPreviewEventInterface : IPreviewEvent
        where T : struct, TEventInterface
    {
        private readonly List<IObserver<T>> observers = [];

        public void HandleEvent(T e)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(e);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observers.Count == 0)
            {
                events.Subscribe(this);
            }
            observers.Add(observer);
            return new Subscription(this, observer);
        }

        private void unsubscribe(IObserver<T> observer)
        {
            observers.Remove(observer);
            if (observers.Count == 0)
            {
                events.Unsubscribe(this);
            }
        }

        private sealed class Subscription(
            Observable<TEventInterface, TPreviewEventInterface, T> observable,
            IObserver<T> observer
        )
            : IDisposable
        {
            public void Dispose() => observable.unsubscribe(observer);
        }
    }

    [MustUseReturnValue]
    public static IReadonlyBinding<T> BindDisplayOnly<T>(
        this IObservable<T> observable, out IDisposable subscription)
    {
        var binding = new Binding<T>();
        subscription = observable.Subscribe(binding.SetFromSource);
        return binding;
    }
    [MustUseReturnValue]
    public static IReadonlyBinding<TOut> BindDisplayOnly<TIn, TOut>(
        this IObservable<TIn> observable, Func<TIn, TOut> selector, out IDisposable subscription)
    {
        var binding = new Binding<TOut>();
        subscription = observable.Select(selector).Subscribe(binding.SetFromSource);
        return binding;
    }
}
