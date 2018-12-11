using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.World;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Mods.Models
{
    sealed class BuildingBlueprint : IBuildingBlueprint
    {
        public string Id { get; }
        public string Name { get; }
        public FootprintGroup FootprintGroup { get; }
        public int ResourceCost { get; }

        public IReadOnlyList<UpgradeTag> Tags { get; }
        private IReadOnlyList<BuildingComponentFactory> componentFactories { get; }

        public BuildingBlueprint(string id, string name, FootprintGroup footprintGroup,
            int resourceCost, IEnumerable<UpgradeTag> tags, IEnumerable<BuildingComponentFactory> componentFactories)
        {
            Id = id;
            Name = name;
            FootprintGroup = footprintGroup;
            ResourceCost = resourceCost;

            Tags = (tags?.ToList() ?? new List<UpgradeTag>()).AsReadOnly();
            this.componentFactories = (componentFactories?.ToList() ?? new List<BuildingComponentFactory>())
                .AsReadOnly();
        }

        public IEnumerable<IComponent<Building>> GetComponentsForBuilding()
            => componentFactories.Select(f => f.TryCreateForBuilding()).NotNull();

        public IEnumerable<IComponent<BuildingGhost>> GetComponentsForGhost()
            => componentFactories.Select(f => f.TryCreateForGhost()).NotNull();

        public IEnumerable<IComponent<BuildingPlaceholder>> GetComponentsForPlaceholder()
            => componentFactories.Select(f => f.TryCreateForPlaceholder()).NotNull();

        public IBuildingBlueprint MakeModifiableInstance()
            => new ModifiableBuildingBlueprint(this, componentFactories);
        
        public bool HasAttributeOfType(AttributeType type)
            => throw new InvalidOperationException("Cannot check attributes on unmodifiable building blueprint.");
        public bool ModifyAttribute(AttributeType type, Modification modification)
            => throw new InvalidOperationException("Cannot modify attributes on unmodifiable building blueprint.");
    }

    sealed class ModifiableBuildingBlueprint : IBuildingBlueprint
    {
        private readonly IBuildingBlueprint reference;
        private readonly IReadOnlyList<BuildingComponentFactory> componentFactories;

        public string Id => reference.Id;
        public string Name => reference.Name;
        public FootprintGroup FootprintGroup => reference.FootprintGroup;
        public int ResourceCost => reference.ResourceCost;
        public IReadOnlyList<UpgradeTag> Tags => reference.Tags;

        public ModifiableBuildingBlueprint(
            IBuildingBlueprint reference, IEnumerable<BuildingComponentFactory> componentFactories)
        {
            this.reference = reference;
            this.componentFactories = componentFactories
                .Select(factory => factory.CreateModifiableFactory())
                .ToList()
                .AsReadOnly();
        }
        
        public IEnumerable<IComponent<Building>> GetComponentsForBuilding()
            => componentFactories.Select(f => f.TryCreateForBuilding()).NotNull();

        public IEnumerable<IComponent<BuildingGhost>> GetComponentsForGhost()
            => componentFactories.Select(f => f.TryCreateForGhost()).NotNull();

        public IEnumerable<IComponent<BuildingPlaceholder>> GetComponentsForPlaceholder()
            => componentFactories.Select(f => f.TryCreateForPlaceholder()).NotNull();
        
        public IBuildingBlueprint MakeModifiableInstance()
            => throw new InvalidOperationException("Modifiableception is not allowed.");
        
        public bool HasAttributeOfType(AttributeType type)
            => componentFactories.Any(factory => factory.HasAttributeOfType(type));

        public bool ModifyAttribute(AttributeType type, Modification modification)
            => componentFactories.Aggregate(
                false,
                (current, factory) => current | factory.ModifyAttribute(type, modification));
    }
}
