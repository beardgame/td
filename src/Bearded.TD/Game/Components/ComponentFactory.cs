using System;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Models;

namespace Bearded.TD.Game.Components
{
    class ComponentFactory : IBlueprint
    {
        public string Id { get; }
        private readonly Func<IComponent<Building>> factoryMethod;
        private readonly Func<IComponent<BuildingGhost>> ghostFactoryMethod;

        public ComponentFactory(
            string id, Func<IComponent<Building>> factoryMethod, Func<IComponent<BuildingGhost>> ghostFactoryMethod = null)
        {
            Id = id;
            this.factoryMethod = factoryMethod;
            this.ghostFactoryMethod = ghostFactoryMethod;
        }

        public IComponent<Building> Create() => factoryMethod();

        public IComponent<BuildingGhost> CreateForGhost() => ghostFactoryMethod?.Invoke();
    }
}
