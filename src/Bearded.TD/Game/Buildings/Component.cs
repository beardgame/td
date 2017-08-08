using System;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    abstract class Component
    {
        public Building Building { get; private set; }

        public void OnAdded(Building building)
        {
            Building = building;
            Initialise();
        }

        protected abstract void Initialise();

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Draw(GeometryManager geometries);
    }

    static class ComponentExtensions
    {
        public static ComponentFactory Factory(
            this Func<Component> factory, Id<ComponentFactory> id, string name)
        {
            return new ComponentFactory(id, name, factory);
        }
    }
}