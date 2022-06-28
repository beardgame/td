using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

readonly record struct UpgradePrerequisites(ImmutableHashSet<string> Tags)
{
    public bool MatchesObject(GameObject obj) => Tags.All(obj.HasTag);
}
