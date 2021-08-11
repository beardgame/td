using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    static class UpgradeExtensions
    {
        public static bool CanApplyTo<T>(this IUpgradeBlueprint upgradeBlueprint, T subject)
            where T : IComponentOwner
            => subject.GetComponents<IComponent>().Any()
                && upgradeBlueprint.Effects.All(effect => effect.CanApplyTo(subject));

        public static void ApplyTo<T>(this IUpgradeBlueprint upgradeBlueprint, T subject)
            where T : IComponentOwner<T>
        {
            DebugAssert.Argument.Satisfies(upgradeBlueprint.CanApplyTo(subject));
            upgradeBlueprint.Effects.ForEach(effect => effect.ApplyTo(subject));
        }
    }
}
