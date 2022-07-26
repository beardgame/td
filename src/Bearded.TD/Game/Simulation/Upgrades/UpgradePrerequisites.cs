using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

readonly record struct UpgradePrerequisites(
    ImmutableHashSet<string> RequiredTags, ImmutableHashSet<string> ForbiddenTags)
{
    public static readonly UpgradePrerequisites Empty =
        new(ImmutableHashSet<string>.Empty, ImmutableHashSet<string>.Empty);

    public bool MatchesObject(GameObject obj) => RequiredTags.All(obj.HasTag) && ForbiddenTags.All(t => !obj.HasTag(t));
}
