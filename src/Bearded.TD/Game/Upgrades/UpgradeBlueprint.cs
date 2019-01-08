using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Components;
using Bearded.TD.Utilities;
using Bearded.Utilities;

namespace Bearded.TD.Game.Upgrades
{
    sealed class UpgradeBlueprint
    {
        public Id<UpgradeBlueprint> Id { get; }
        public string Name { get; }
        private readonly ImmutableList<IUpgradeEffect> effects;

        public UpgradeBlueprint(Id<UpgradeBlueprint> id, string name, IEnumerable<IUpgradeEffect> effects)
        {
            Id = id;
            Name = name;
            this.effects = ImmutableList.CreateRange(effects);
        }

        public bool CanApplyTo<T>(ComponentCollection<T> components)
            => components.Components.Count > 0 && effects.TrueForAll(effect => effect.CanApplyTo(components));

        public void ApplyTo<T>(ComponentCollection<T> components)
        {
            DebugAssert.Argument.Satisfies(CanApplyTo(components));
            effects.ForEach(effect => effect.ApplyTo(components));
        }
    }
}
