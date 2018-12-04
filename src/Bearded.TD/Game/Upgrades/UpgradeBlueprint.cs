﻿using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Components;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Upgrades
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

        public bool CanApplyTo<T>(ComponentCollection<T> components)
            => components.Components.Count > 0 && effects.TrueForAll(effect => effect.CanApplyTo(components));

        public void ApplyTo<T>(ComponentCollection<T> components)
        {
            DebugAssert.Argument.Satisfies(CanApplyTo(components));
            effects.ForEach(effect => effect.ApplyTo(components));
        }
    }
}
