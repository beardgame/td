using System.Collections.Immutable;

namespace Bearded.TD.Game.Simulation.Upgrades;

static class Upgrade
{
    public static IUpgrade FromEffects(params IUpgradeEffect[] effects) =>
        new SimpleUpgrade(effects.ToImmutableArray());

    private sealed record SimpleUpgrade(ImmutableArray<IUpgradeEffect> Effects) : IUpgrade;
}
