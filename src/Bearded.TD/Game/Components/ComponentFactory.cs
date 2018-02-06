using System;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Serialization.Models;

namespace Bearded.TD.Game.Components
{
    class BuildingComponentFactory
    {
        private readonly IComponentFactory<Building> buildingFactory;
        private readonly IComponentFactory<BuildingGhost> ghostFactory;
        private readonly IComponentFactory<BuildingPlaceholder> placeholderFactory;

        public BuildingComponentFactory(IBuildingComponent parameters,
            IComponentFactory<Building> buildingFactory,
            IComponentFactory<BuildingGhost> ghostFactory,
            IComponentFactory<BuildingPlaceholder> placeholderFactory)
        {
            this.buildingFactory = parameters.OnBuilding ? buildingFactory : null;
            this.ghostFactory = parameters.OnGhost ? ghostFactory : null;
            this.placeholderFactory = parameters.OnPlaceholder ? placeholderFactory : null;
        }

        public IComponent<Building> TryCreateForBuilding() => buildingFactory?.Create();

        public IComponent<BuildingGhost> TryCreateForGhost() => ghostFactory?.Create();

        public IComponent<BuildingPlaceholder> TryCreateForPlaceholder() => placeholderFactory?.Create();
    }

    class ComponentFactory<TOwner, TComponentParameters> : IComponentFactory<TOwner>
    {
        private readonly TComponentParameters parameters;
        private readonly Func<TComponentParameters, IComponent<TOwner>> factory;

        public ComponentFactory(TComponentParameters parameters, Func<TComponentParameters, IComponent<TOwner>> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public IComponent<TOwner> Create() => factory(parameters);
    }

    interface IComponentFactory<TOwner>
    {
        IComponent<TOwner> Create();
    }
}
