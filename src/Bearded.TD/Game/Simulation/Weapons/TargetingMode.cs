using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Weapons;

static class TargetingMode
{
    public static readonly ITargetingMode Default = new DefaultTargetingMode();

    private sealed class DefaultTargetingMode : ITargetingMode
    {
        public string Name => "Default";

        public GameObject? SelectTarget(IEnumerable<GameObject> candidates) => candidates.FirstOrDefault();
    }

    public static readonly ImmutableArray<ITargetingMode> All = ImmutableArray.Create(Default);
}
