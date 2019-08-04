using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Game.Upgrades
{
    [Obsolete]
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
            Effects = ImmutableList.CreateRange(effects);
        }
    }
}
