using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Upgrades;

namespace Bearded.TD.Content.Models
{
    sealed class UpgradeBlueprint : IUpgradeBlueprint
    {
        public string Id { get; }
        public string Name { get; }
        public double Cost { get; }
        public IEnumerable<IUpgradeEffect> Effects { get; }

        public UpgradeBlueprint(string id, string name, double cost, IEnumerable<IUpgradeEffect> effects)
        {
            Id = id;
            Name = name;
            Cost = cost;
            Effects = effects?.ToImmutableList() ?? ImmutableList<IUpgradeEffect>.Empty;
        }
    }
}
