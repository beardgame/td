using System;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game.Buildings
{
    class ComponentFactory : IBlueprint
    {
        public string Id { get; }
        private readonly Func<Component> factoryMethod;
        private readonly Func<Component<BuildingGhost>> ghostFactoryMethod;

        public ComponentFactory(
            string id, Func<Component> factoryMethod, Func<Component<BuildingGhost>> ghostFactoryMethod = null)
        {
            Id = id;
            this.factoryMethod = factoryMethod;
            this.ghostFactoryMethod = ghostFactoryMethod;
        }

        public Component Create() => factoryMethod();

        public Component<BuildingGhost> CreateForGhost() => ghostFactoryMethod?.Invoke();
    }
}
