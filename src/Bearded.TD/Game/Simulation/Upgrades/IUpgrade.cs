using System.Collections.Immutable;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgrade
{
    ImmutableArray<IUpgradeEffect> Effects { get; }
}
