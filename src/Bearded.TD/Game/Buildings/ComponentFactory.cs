using System;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game.Buildings
{
    class ComponentFactory : IBlueprint
    {
        public string Name { get; }
        private readonly Func<Component> factoryMethod;
        private readonly Func<IComponent<BuildingGhost>> ghostFactoryMethod;

        public ComponentFactory(
            string name, Func<Component> factoryMethod, Func<IComponent<BuildingGhost>> ghostFactoryMethod = null)
        {
            this.factoryMethod = factoryMethod;
            this.ghostFactoryMethod = ghostFactoryMethod;
            Name = name;
        }

        public Component Create() => factoryMethod();

        public IComponent<BuildingGhost> CreateForGhost() => ghostFactoryMethod?.Invoke();
    }
}
