using System;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Mods.Serialization.Models;

namespace Bearded.TD.Game.Components
{
    class BuildingComponentFactory<TComponentParameters> : IBuildingComponentFactory
    {
        private readonly BuildingComponent<TComponentParameters> parameters;
        private readonly ComponentFactory<Building, TComponentParameters> buildingFactory;
        private readonly ComponentFactory<BuildingGhost, TComponentParameters> ghostFactory;
        private readonly ComponentFactory<BuildingPlaceholder, TComponentParameters> placeholderFactory;

        public BuildingComponentFactory(BuildingComponent<TComponentParameters> parameters,
            ComponentFactory<Building, TComponentParameters> buildingFactory,
            ComponentFactory<BuildingGhost, TComponentParameters> ghostFactory,
            ComponentFactory<BuildingPlaceholder, TComponentParameters> placeholderFactory)
        {
            this.parameters = parameters;
            this.buildingFactory = buildingFactory;
            this.ghostFactory = ghostFactory;
            this.placeholderFactory = placeholderFactory;
        }

    }

    interface IBuildingComponentFactory
    {
        // create for ghost? yay nay
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
