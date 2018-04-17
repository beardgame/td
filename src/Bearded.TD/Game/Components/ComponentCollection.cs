using System.Collections.Generic;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    class ComponentCollection<TOwner>
    {
        private readonly List<IComponent<TOwner>> components = new List<IComponent<TOwner>>();

        public IReadOnlyCollection<IComponent<TOwner>> Components { get; }

        public ComponentCollection()
        {
            Components = components.AsReadOnly();
        }

        public void Add(TOwner owner, IEnumerable<IComponent<TOwner>> newComponents)
        {
            foreach (var component in newComponents)
                Add(owner, component);
        }

        public void Add(TOwner owner, IComponent<TOwner> component)
        {
            components.Add(component);
            component.OnAdded(owner);
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
