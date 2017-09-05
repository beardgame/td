using System;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Collections;

namespace Bearded.TD.Game.Buildings
{
    class ComponentFactory : IIdable<ComponentFactory>, INamed
    {
        public Id<ComponentFactory> Id { get; }
        public string Name { get; }
        private readonly Func<Component> factoryMethod;
        private readonly Func<Component<IPositionableGameObject>> ghostFactoryMethod;

        public ComponentFactory(Id<ComponentFactory> id, string name,
            Func<Component> factoryMethod, Func<Component<IPositionableGameObject>> ghostFactoryMethod = null)
        {
            this.factoryMethod = factoryMethod;
            this.ghostFactoryMethod = ghostFactoryMethod;
            Id = id;
            Name = name;
        }

        public Component Create() => factoryMethod();

        public IComponent<IPositionableGameObject> CreateForGhost() => ghostFactoryMethod?.Invoke();
    }
}
