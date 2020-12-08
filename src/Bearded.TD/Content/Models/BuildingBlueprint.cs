using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.GameState.Buildings;
using Bearded.TD.Game.GameState.Components;
using Bearded.TD.Game.GameState.Resources;
using Bearded.TD.Game.GameState.Upgrades;
using Bearded.TD.Game.GameState.World;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Content.Models
{
    sealed class BuildingBlueprint : IBuildingBlueprint
    {
        public ModAwareId Id { get; }
        public string Name { get; }
        public FootprintGroup FootprintGroup { get; }
        public ResourceAmount ResourceCost { get; }

        public IReadOnlyList<UpgradeTag> Tags { get; }
        private IReadOnlyList<BuildingComponentFactory> componentFactories { get; }

        public BuildingBlueprint(ModAwareId id, string name, FootprintGroup footprintGroup,
            ResourceAmount resourceCost, IEnumerable<UpgradeTag> tags, IEnumerable<BuildingComponentFactory> componentFactories)
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
    }
}
