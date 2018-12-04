using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Game.Buildings;
using Bearded.TD.Game.Components;
using Bearded.TD.Game.Upgrades;
using Bearded.TD.Game.World;
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
    }
}
