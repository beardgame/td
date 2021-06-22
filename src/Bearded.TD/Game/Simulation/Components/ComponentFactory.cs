using System;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Components
{
    sealed class BuildingComponentFactory
    {
        private readonly IComponentFactory<Building>? buildingFactory;
        private readonly IComponentFactory<BuildingGhost>? ghostFactory;

        public BuildingComponentFactory(IBuildingComponent parameters,
            IComponentFactory<Building> buildingFactory,
            IComponentFactory<BuildingGhost> ghostFactory)
        {
            this.buildingFactory = parameters.OnBuilding ? buildingFactory : null;
            this.ghostFactory = parameters.OnGhost ? ghostFactory : null;
        }

        public IComponent<Building>? TryCreateForBuilding() => buildingFactory?.Create();

        public IComponent<BuildingGhost>? TryCreateForGhost() => ghostFactory?.Create();
    }

    sealed class ComponentFactory<TOwner, TComponentParameters> : IComponentFactory<TOwner>
        where TComponentParameters : IParametersTemplate<TComponentParameters>
    {
        private readonly TComponentParameters parameters;
        private readonly Func<TComponentParameters, IComponent<TOwner>> factory;

        public ComponentFactory(TComponentParameters parameters, Func<TComponentParameters, IComponent<TOwner>> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public IComponent<TOwner> Create() => factory(parameters);

        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => effect.CanApplyTo(parameters);
    }

    interface IComponentFactory<in TOwner>
    {
        IComponent<TOwner> Create();
        bool CanApplyUpgradeEffect(IUpgradeEffect effect);
    }
}
