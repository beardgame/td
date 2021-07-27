using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Game.Simulation.World;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Content.Models
{
    sealed class BuildingBlueprint : IBuildingBlueprint
    {
        public ModAwareId Id { get; }
        public string Name { get; }
        public ResourceAmount ResourceCost { get; }

        public IReadOnlyList<UpgradeTag> Tags { get; }
        private IReadOnlyList<BuildingComponentFactory> componentFactories { get; }

        public BuildingBlueprint(
            ModAwareId id,
            string name,
            ResourceAmount resourceCost,
            IEnumerable<UpgradeTag> tags,
            IEnumerable<BuildingComponentFactory> componentFactories)
        {
            Id = id;
            Name = name;
            ResourceCost = resourceCost;

            Tags = tags.ToImmutableArray();
            this.componentFactories = componentFactories.ToImmutableArray();
        }

        public IEnumerable<IComponent<Building>> GetComponentsForBuilding()
            => componentFactories.Select(f => f.TryCreateForBuilding()).NotNull();

        public IEnumerable<IComponent<BuildingGhost>> GetComponentsForGhost()
            => componentFactories.Select(f => f.TryCreateForGhost()).NotNull();
    }
}
