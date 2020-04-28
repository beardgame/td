﻿using System;
using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Shared.TechEffects;

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

    sealed class ComponentFactory<TOwner, TComponentParameters> : IComponentFactory<TOwner>
        where TComponentParameters : IParametersTemplate<TComponentParameters>
    {
        private readonly TComponentParameters parameters;
        private readonly Func<TComponentParameters, object> factory;

        public ComponentFactory(TComponentParameters parameters, Func<TComponentParameters, object> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public IComponent<TOwner> Create() => (IComponent<TOwner>) factory(parameters);

        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => effect.CanApplyTo(parameters);
    }

    interface IComponentFactory<in TOwner>
    {
        IComponent<TOwner> Create();
        bool CanApplyUpgradeEffect(IUpgradeEffect effect);
    }
}
