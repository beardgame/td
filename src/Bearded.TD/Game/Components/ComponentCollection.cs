using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    class ComponentCollection<TOwner>
    {
        private readonly TOwner owner;
        private readonly List<IComponent<TOwner>> components = new List<IComponent<TOwner>>();

        public IReadOnlyCollection<IComponent<TOwner>> Components { get; }

        public ComponentCollection(TOwner owner)
        {
            this.owner = owner;
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
            component.OnAdded(owner);
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

        public void Draw(GeometryManager geometries)
        {
            foreach (var component in components)
                component.Draw(geometries);
        }
    }
}
