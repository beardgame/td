using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components
{
    sealed class ComponentCollection<TOwner>
    {
        private readonly TOwner owner;
        private readonly ComponentEvents events;
        private readonly List<IComponent<TOwner>> components = new();

        private bool deferComponentCollectionChanges;
        private readonly Queue<IComponent<TOwner>> componentsToBeAdded = new();

        public ComponentCollection(TOwner owner, ComponentEvents events)
        {
            this.owner = owner;
            this.events = events;
        }

        public void Add(IEnumerable<IComponent<TOwner>> newComponents)
        {
            foreach (var component in newComponents)
            {
                Add(component);
            }
        }

        public void Add(IComponent<TOwner> component)
        {
            if (deferComponentCollectionChanges)
            {
                componentsToBeAdded.Enqueue(component);
                return;
            }
            components.Add(component);
            component.OnAdded(owner, events);
        }

        public IEnumerable<T> Get<T>()
        {
            return components.OfType<T>();
        }

        public void Update(TimeSpan elapsedTime)
        {
            deferComponentCollectionChanges = true;
            foreach (var component in components)
            {
                component.Update(elapsedTime);
            }
            deferComponentCollectionChanges = false;
            while (componentsToBeAdded.Count > 0)
            {
                Add(componentsToBeAdded.Dequeue());
            }
        }

        public void Draw(CoreDrawers drawers)
        {
            foreach (var component in components)
            {
                component.Draw(drawers);
            }
        }
    }
}
