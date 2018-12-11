using System;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Mods.Serialization.Models;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Components
{
    class BuildingComponentFactory : IAttributeModifiable
    {
        private readonly IBuildingComponent parameters;
        protected IComponentFactory<Building> BuildingFactory { get; }
        private readonly IComponentFactory<BuildingGhost> ghostFactory;
        private readonly IComponentFactory<BuildingPlaceholder> placeholderFactory;

        public BuildingComponentFactory(IBuildingComponent parameters,
            IComponentFactory<Building> buildingFactory,
            IComponentFactory<BuildingGhost> ghostFactory,
            IComponentFactory<BuildingPlaceholder> placeholderFactory)
        {
            this.parameters = parameters;
            BuildingFactory = parameters.OnBuilding ? buildingFactory : null;
            this.ghostFactory = parameters.OnGhost ? ghostFactory : null;
            this.placeholderFactory = parameters.OnPlaceholder ? placeholderFactory : null;
        }

        public IComponent<Building> TryCreateForBuilding() => BuildingFactory?.Create();

        public IComponent<BuildingGhost> TryCreateForGhost() => ghostFactory?.Create();

        public IComponent<BuildingPlaceholder> TryCreateForPlaceholder() => placeholderFactory?.Create();
        
        public BuildingComponentFactory CreateModifiableFactory()
            => new ModifiableBuildingComponentFactory(parameters, BuildingFactory, ghostFactory, placeholderFactory);
        
        public virtual bool HasAttributeOfType(AttributeType type)
            => throw new InvalidOperationException("Cannot check attributes on unmodifiable building component factory.");
        public virtual bool ModifyAttribute(AttributeType type, Modification modification)
            => throw new InvalidOperationException("Cannot modify attributes on unmodifiable building component factory.");
    }

    class ModifiableBuildingComponentFactory : BuildingComponentFactory
    {
        public ModifiableBuildingComponentFactory(
            IBuildingComponent parameters,
            IComponentFactory<Building> buildingFactory,
            IComponentFactory<BuildingGhost> ghostFactory,
            IComponentFactory<BuildingPlaceholder> placeholderFactory)
            : base(
                parameters,
                buildingFactory?.CreateModifiableFactory(),
                ghostFactory?.CreateModifiableFactory(),
                placeholderFactory?.CreateModifiableFactory()) { }
        
        public override bool HasAttributeOfType(AttributeType type)
            => BuildingFactory?.HasAttributeOfType(type) ?? false;

        public override bool ModifyAttribute(AttributeType type, Modification modification)
            => BuildingFactory?.ModifyAttribute(type, modification) ?? false;
    }

    class ComponentFactory<TOwner, TComponentParameters> : IComponentFactory<TOwner>
        where TComponentParameters : IParametersTemplate<TComponentParameters>
    {
        protected TComponentParameters Parameters { get; }
        private readonly Func<TComponentParameters, IComponent<TOwner>> factory;

        public ComponentFactory(TComponentParameters parameters, Func<TComponentParameters, IComponent<TOwner>> factory)
        {
            Parameters = parameters;
            this.factory = factory;
        }

        public IComponent<TOwner> Create() => factory(Parameters);

        public virtual bool HasAttributeOfType(AttributeType type)
            => throw new InvalidOperationException("Cannot check attributes on unmodifiable component factory.");
        public virtual bool ModifyAttribute(AttributeType type, Modification modification)
            => throw new InvalidOperationException("Cannot modify attributes on unmodifiable component factory.");

        public IComponentFactory<TOwner> CreateModifiableFactory()
            => new ModifiableComponentFactory<TOwner, TComponentParameters>(Parameters, factory);
    }

    class ModifiableComponentFactory<TOwner, TComponentParameters> : ComponentFactory<TOwner, TComponentParameters>
        where TComponentParameters : IParametersTemplate<TComponentParameters>
    {
        public ModifiableComponentFactory(
                TComponentParameters parameters, Func<TComponentParameters, IComponent<TOwner>> factory)
            : base(parameters.CreateModifiableInstance(), factory) { }

        public override bool HasAttributeOfType(AttributeType type) => Parameters.HasAttributeOfType(type);

        public override bool ModifyAttribute(AttributeType type, Modification modification) =>
            Parameters.ModifyAttribute(type, modification);
    }

    interface IComponentFactory<TOwner> : IAttributeModifiable
    {
        IComponent<TOwner> Create();
        IComponentFactory<TOwner> CreateModifiableFactory();
    }
}
