using System;
using System.Linq;
using Bearded.TD.Shared.Events;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Components
{
    static class ComponentDependencies
    {
        public static void DependDynamic<T>(IComponentOwner owner, ComponentEvents events, Action<T?> consumer)
            where T : class
        {
            var currentDependency = owner.GetComponents<T>().LastOrDefault();
            if (currentDependency != null)
            {
                consumer(currentDependency);
            }

            var listener = new DynamicDependencyListener<T>();
            listener.AddedComponent += dep =>
            {
                currentDependency = dep;
                consumer(dep);
            };
            listener.RemovedComponent += dep =>
            {
                if (dep != currentDependency)
                    return;
                currentDependency = owner.GetComponents<T>().LastOrDefault();
                consumer(currentDependency);
            };
            events.Subscribe<ComponentAdded>(listener);
            events.Subscribe<ComponentRemoved>(listener);
        }

        public static void Depend<T>(IComponentOwner owner, ComponentEvents events, Action<T> consumer)
        {
            var found = owner.GetComponents<T>().FirstOrDefault();
            if (found != null)
            {
                consumer(found);
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

        public static void Depend<T1, T2>(IComponentOwner owner, ComponentEvents events, Action<T1, T2> consumer)
        {
            var dep1Found = false;
            var dep2Found = false;
            var dep1 = default(T1);
            var dep2 = default(T2);

            Depend<T1>(owner, events, dep =>
            {
                dep1Found = true;
                dep1 = dep;
                collectDependencies();
            });
            Depend<T2>(owner, events, dep =>
            {
                dep2Found = true;
                dep2 = dep;
                collectDependencies();
            });

            void collectDependencies()
            {
                if (dep1Found && dep2Found)
                {
                    consumer(dep1, dep2);
                }
            }
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

        private sealed class DynamicDependencyListener<T> : IListener<ComponentAdded>, IListener<ComponentRemoved>
        {
            public event GenericEventHandler<T>? AddedComponent;
            public event GenericEventHandler<T>? RemovedComponent;

            public void HandleEvent(ComponentAdded @event)
            {
                if (@event.Component is not T dependency) return;

                AddedComponent?.Invoke(dependency);
            }

            public void HandleEvent(ComponentRemoved @event)
            {
                if (@event.Component is not T dependency) return;

                RemovedComponent?.Invoke(dependency);
            }
        }
    }
}
