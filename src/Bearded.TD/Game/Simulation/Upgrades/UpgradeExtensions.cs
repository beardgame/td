using System.Linq;
using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Upgrades;

static class UpgradeExtensions
{
    public static bool CanApplyTo(this IUpgradeBlueprint upgradeBlueprint, ComponentGameObject subject)
        => subject.GetComponents<IComponent>().Any()
            && upgradeBlueprint.Effects.All(effect => effect.CanApplyTo(subject));

    public static void ApplyTo(this IUpgradeBlueprint upgradeBlueprint, ComponentGameObject subject)
    {
        DebugAssert.Argument.Satisfies(upgradeBlueprint.CanApplyTo(subject));
        upgradeBlueprint.Effects.ForEach(effect => effect.ApplyTo(subject));
    }
}
