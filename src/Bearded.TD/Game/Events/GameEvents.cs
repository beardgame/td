using System;
using System.Collections.Generic;

namespace Bearded.TD.Game.Events
{
    abstract class GameEvents<TEventInterface> where TEventInterface : IEvent
    {
        private readonly Dictionary<Type, object> listenerLists = new Dictionary<Type, object>();

        public void Subscribe<TEvent>(IListener<TEvent> listener)
            where TEvent : TEventInterface
        {
            getListeners<TEvent>().Add(listener);
        }

        public void Unsubscribe<TEvent>(IListener<TEvent> listener)
            where TEvent : TEventInterface
        {
            if (hasListeners<TEvent>(out var listeners))
                listeners.Remove(listener);
        }

        public void Send<TEvent>(TEvent @event)
            where TEvent : TEventInterface
        {
            if (hasListeners<TEvent>(out var listeners))
                send(@event, listeners);
        }

        private static void send<TEvent>(TEvent @event, List<IListener<TEvent>> listeners)
            where TEvent : TEventInterface
        {
            foreach (var listener in listeners)
            {
                listener.HandleEvent(@event);
            }
        }

        private List<IListener<TEvent>> getListeners<TEvent>()
            where TEvent : TEventInterface
        {
            if (hasListeners(out List<IListener<TEvent>> listeners))
                return listeners;

            listeners = new List<IListener<TEvent>>();
            listenerLists.Add(typeof(TEvent), listeners);

            return listeners;
        }

        private bool hasListeners<TEvent>(out List<IListener<TEvent>> listeners)
            where TEvent : TEventInterface
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
