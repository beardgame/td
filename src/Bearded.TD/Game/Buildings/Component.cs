using System;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Buildings
{
    interface IComponent<in TOwner>
    {
        void OnAdded(TOwner owner);
        
        void Update(TimeSpan elapsedTime);

        void Draw(GeometryManager geometries);
    }

    abstract class Component<TOwner> : IComponent<TOwner>
    {
        public TOwner Owner { get; private set; }

        public void OnAdded(TOwner owner)
        {
            Owner = owner;
            Initialise();
        }

        protected abstract void Initialise();

        public abstract void Update(TimeSpan elapsedTime);

        public abstract void Draw(GeometryManager geometries);
    }

    abstract class Component : Component<Building>
    {
        public Building Building => Owner;
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