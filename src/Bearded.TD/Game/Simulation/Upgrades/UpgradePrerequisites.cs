using System.Collections.Immutable;

namespace Bearded.TD.Game.Simulation.Upgrades;

readonly record struct UpgradePrerequisites(
    ImmutableHashSet<string> RequiredTags, ImmutableHashSet<string> ForbiddenTags)
{
    public static readonly UpgradePrerequisites Empty =
        new(ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty);
}
