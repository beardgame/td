using System;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game.Components
{
    class ComponentFactory : IBlueprint
    {
        public string Id { get; }
        private readonly Func<Component> factoryMethod;
        private readonly Func<IComponent<BuildingGhost>> ghostFactoryMethod;

        public ComponentFactory(
            string id, Func<Component> factoryMethod, Func<IComponent<BuildingGhost>> ghostFactoryMethod = null)
        {
            Id = id;
            this.factoryMethod = factoryMethod;
            this.ghostFactoryMethod = ghostFactoryMethod;
        }

        public Component Create() => factoryMethod();

        public IComponent<BuildingGhost> CreateForGhost() => ghostFactoryMethod?.Invoke();
    }
}
