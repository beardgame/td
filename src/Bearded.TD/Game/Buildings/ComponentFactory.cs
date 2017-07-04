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

        public ComponentFactory(Id<ComponentFactory> id, string name, Func<Component> factoryMethod)
        {
            this.factoryMethod = factoryMethod;
            Id = id;
            Name = name;
        }

        public Component Create() => factoryMethod();
    }
}
