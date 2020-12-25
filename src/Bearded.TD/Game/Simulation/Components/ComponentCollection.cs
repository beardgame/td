using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Components
{
    sealed class ComponentCollection<TOwner>
    {
        private readonly TOwner owner;
        private readonly ComponentEvents events;
        private readonly List<IComponent<TOwner>> components = new List<IComponent<TOwner>>();

        public IReadOnlyCollection<IComponent<TOwner>> Components { get; }

        public ComponentCollection(TOwner owner, ComponentEvents events)
        {
            this.owner = owner;
            this.events = events;
            Components = components.AsReadOnly();
        }

        public void Add(IEnumerable<IComponent<TOwner>> newComponents)
        {
            foreach (var component in newComponents)
                Add(component);
        }

        public void Add(IComponent<TOwner> component)
        {
            components.Add(component);
            component.OnAdded(owner, events);
        }

        public IEnumerable<T> Get<T>()
        {
            return components.OfType<T>();
        }

        public void Update(TimeSpan elapsedTime)
        {
            foreach (var component in components)
                component.Update(elapsedTime);
        }

        public void Draw(CoreDrawers geometries)
        {
            foreach (var component in components)
                component.Draw(geometries);
        }
    }
}
