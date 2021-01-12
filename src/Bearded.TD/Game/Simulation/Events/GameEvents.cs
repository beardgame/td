using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Bearded.TD.Game.Simulation.Events
{
    abstract class GameEvents<TEventInterface> where TEventInterface : IEvent
    {
        private readonly Dictionary<Type, object> listenerLists = new();

        public void Subscribe<TEvent>(IListener<TEvent> listener)
            where TEvent : struct, TEventInterface
        {
            getListeners<TEvent>().Add(listener);
        }

        public void Unsubscribe<TEvent>(IListener<TEvent> listener)
            where TEvent : struct, TEventInterface
        {
            if (hasListeners<TEvent>(out var listeners))
                listeners.Remove(listener);
        }

        public void Send<TEvent>(TEvent @event)
            where TEvent : struct, TEventInterface
        {
            if (hasListeners<TEvent>(out var listeners))
                send(@event, listeners);
        }

        private static void send<TEvent>(TEvent @event, IEnumerable<IListener<TEvent>> listeners)
            where TEvent : struct, TEventInterface
        {
            foreach (var listener in listeners)
            {
                listener.HandleEvent(@event);
            }
        }

        private List<IListener<TEvent>> getListeners<TEvent>()
            where TEvent : struct, TEventInterface
        {
            if (hasListeners(out List<IListener<TEvent>> listeners))
            {
                return listeners;
            }

            listeners = new List<IListener<TEvent>>();
            listenerLists.Add(typeof(TEvent), listeners);

            return listeners;
        }

        private bool hasListeners<TEvent>([NotNullWhen(returnValue: true)] out List<IListener<TEvent>>? listeners)
            where TEvent : struct, TEventInterface
        {
            if (listenerLists.TryGetValue(typeof(TEvent), out var listAsObject))
            {
                listeners = (List<IListener<TEvent>>)listAsObject;
                return true;
            }

            listeners = null;
            return false;
        }
    }
}
