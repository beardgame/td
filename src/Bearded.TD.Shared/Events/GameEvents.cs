using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Bearded.TD.Shared.Events
{
    public abstract class GameEvents<TEventInterface, TPreviewEventInterface>
        where TEventInterface : IEvent
        where TPreviewEventInterface: IPreviewEvent
    {
        private readonly Dictionary<Type, object> listenerLists = new();
        private readonly Dictionary<Type, object> previewListenerLists = new();

        public void Subscribe<TEvent>(IListener<TEvent> listener)
            where TEvent : struct, TEventInterface
        {
            getListeners<TEvent>().Add(listener);
        }

        public void Subscribe<TEvent>(IPreviewListener<TEvent> listener)
            where TEvent : struct, TPreviewEventInterface
        {
            getPreviewListeners<TEvent>().Add(listener);
        }

        public void Unsubscribe<TEvent>(IListener<TEvent> listener)
            where TEvent : struct, TEventInterface
        {
            if (tryGetListeners<TEvent>(out var listeners))
            {
                listeners.Remove(listener);
            }
        }

        public void Unsubscribe<TEvent>(IPreviewListener<TEvent> listener)
            where TEvent : struct, TPreviewEventInterface
        {
            if (tryGetPreviewListeners<TEvent>(out var listeners))
            {
                listeners.Remove(listener);
            }
        }

        public void Send<TEvent>(TEvent @event)
            where TEvent : struct, TEventInterface
        {
            if (tryGetListeners<TEvent>(out var listeners))
            {
                foreach (var listener in listeners)
                {
                    listener.HandleEvent(@event);
                }
            }
        }

        public void Preview<TEvent>(ref TEvent @event)
            where TEvent : struct, TPreviewEventInterface
        {
            if (tryGetPreviewListeners<TEvent>(out var listeners))
            {
                foreach (var listener in listeners)
                {
                    listener.PreviewEvent(ref @event);
                }
            }
        }

        private List<IListener<TEvent>> getListeners<TEvent>()
            where TEvent : struct, TEventInterface
        {
            if (tryGetListeners(out List<IListener<TEvent>> listeners))
            {
                return listeners;
            }

            listeners = new List<IListener<TEvent>>();
            listenerLists.Add(typeof(TEvent), listeners);

            return listeners;
        }

        private bool tryGetListeners<TEvent>([NotNullWhen(true)] out List<IListener<TEvent>>? listeners)
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

        private List<IPreviewListener<TEvent>> getPreviewListeners<TEvent>()
            where TEvent : struct, TPreviewEventInterface
        {
            if (tryGetPreviewListeners(out List<IPreviewListener<TEvent>> listeners))
            {
                return listeners;
            }

            listeners = new List<IPreviewListener<TEvent>>();
            previewListenerLists.Add(typeof(TEvent), listeners);

            return listeners;
        }

        private bool tryGetPreviewListeners<TEvent>(
            [NotNullWhen(returnValue: true)] out List<IPreviewListener<TEvent>>? listeners)
            where TEvent : struct, TPreviewEventInterface
        {
            if (previewListenerLists.TryGetValue(typeof(TEvent), out var listAsObject))
            {
                listeners = (List<IPreviewListener<TEvent>>)listAsObject;
                return true;
            }

            listeners = null;
            return false;
        }
    }
}
