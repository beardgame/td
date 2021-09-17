using System;
using System.Linq;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Components
{
    static class ComponentDependencies
    {
        public static void Depend<T>(IComponentOwner owner, ComponentEvents events, Action<T> consumer)
        {
            var found = owner.GetComponents<T>().ToList();
            if (found.Count > 0)
            {
                consumer(found[0]);
                return;
            }

            var listener = new DependencyListener<T>();
            listener.Resolved += dep =>
            {
                consumer(dep);
                // TODO: we should unsubscribe after this. Luckily component added events happen rarely.
            };
            events.Subscribe(listener);
        }

        private sealed class DependencyListener<T> : IListener<ComponentAdded>
        {
            private bool isResolved;
            public event GenericEventHandler<T>? Resolved;

            public void HandleEvent(ComponentAdded @event)
            {
                if (isResolved || @event.Component is not T dependency) return;

                Resolved?.Invoke(dependency);
                isResolved = true;
            }
        }
    }
}
