using System;
using System.Collections.Generic;

namespace Bearded.TD.Game
{
    interface IEvent
    {
    }

    interface IListener<in TEvent>
        where TEvent : IEvent
    {
        void HandleEvent(TEvent @event);
    }

    sealed class GameEvents
    {
        private readonly Dictionary<Type, object> listenerLists = new Dictionary<Type, object>();

        public void Subscribe<TEvent>(IListener<TEvent> listener)
            where TEvent : IEvent
        {
            getListeners<TEvent>().Add(listener);
        }

        public void Unsubscribe<TEvent>(IListener<TEvent> listener)
            where TEvent : IEvent
        {
            if (hasListeners<TEvent>(out var listeners))
                listeners.Remove(listener);
        }

        public void Send<TEvent>(TEvent @event)
            where TEvent : IEvent
        {
            if (hasListeners<TEvent>(out var listeners))
                send(@event, listeners);
        }

        private static void send<TEvent>(TEvent @event, List<IListener<TEvent>> listeners)
            where TEvent : IEvent
        {
            foreach (var listener in listeners)
            {
                listener.HandleEvent(@event);
            }
        }

        private List<IListener<TEvent>> getListeners<TEvent>()
            where TEvent : IEvent
        {
            if (hasListeners(out List<IListener<TEvent>> listeners))
                return listeners;

            listeners = new List<IListener<TEvent>>();
            listenerLists.Add(typeof(TEvent), listeners);

            return listeners;
        }
        
        private bool hasListeners<TEvent>(out List<IListener<TEvent>> listeners)
            where TEvent : IEvent
        {
            if (listenerLists.TryGetValue(typeof(TEvent), out var listAsObject))
            {
                listeners = null;
                return false;
            }

            listeners = (List<IListener<TEvent>>)listAsObject;
            return true;
        }
    }
}
