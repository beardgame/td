using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Game.Technologies
{
    sealed class UpgradeBlueprint
    {
        public string Name { get; }
        private readonly ImmutableList<IUpgradeEffect> effects;

        public UpgradeBlueprint(string name, IEnumerable<IUpgradeEffect> effects)
        {
            Name = name;
            this.effects = ImmutableList.CreateRange(effects);
        }
    }
}
