using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    static class UpgradeExtensions
    {
        public static bool CanApplyTo<T>(this IUpgradeBlueprint upgradeBlueprint, ComponentCollection<T> components)
            => components.Components.Count > 0 && upgradeBlueprint.Effects.All(effect => effect.CanApplyTo(components));

        public static void ApplyTo<T>(this IUpgradeBlueprint upgradeBlueprint, ComponentCollection<T> components)
        {
            DebugAssert.Argument.Satisfies(upgradeBlueprint.CanApplyTo(components));
            upgradeBlueprint.Effects.ForEach(effect => effect.ApplyTo(components));
        }
    }
}
