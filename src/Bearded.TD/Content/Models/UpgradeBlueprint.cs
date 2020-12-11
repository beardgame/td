using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Game.Simulation.Upgrades;

namespace Bearded.TD.Content.Models
{
    sealed class UpgradeBlueprint : IUpgradeBlueprint
    {
        public ModAwareId Id { get; }
        public string Name { get; }
        public ResourceAmount Cost { get; }
        public IEnumerable<IUpgradeEffect> Effects { get; }

        public UpgradeBlueprint(ModAwareId id, string name, ResourceAmount cost, IEnumerable<IUpgradeEffect> effects)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Effects = effects?.ToImmutableArray() ?? ImmutableArray<IUpgradeEffect>.Empty;
        }
    }
}
