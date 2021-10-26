using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Buildings;
using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Content.Models
{
    sealed class BuildingBlueprint : IBuildingBlueprint
    {
        public ModAwareId Id { get; }
        private IReadOnlyList<IComponentFactory<Building>> componentFactories { get; }

        public BuildingBlueprint(
            ModAwareId id,
            IEnumerable<IComponentFactory<Building>> componentFactories)
        {
            Id = id;
            this.componentFactories = componentFactories.ToImmutableArray();
        }

        public IEnumerable<IComponent<Building>> GetComponents() => componentFactories.Select(f => f.Create());
    }
}
