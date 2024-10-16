using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Simulation.StatusDisplays;

sealed partial class StatusTracker
{
    private readonly List<HitPointsBar> hitPointsBars = [];
    public ReadOnlyCollection<HitPointsBar> HitPointsBars { get; }

    public void AddHitPointsBar(HitPointsBar bar)
    {
        hitPointsBars.Add(bar);
        hitPointsBars.Sort(Comparer.Comparing<HitPointsBar, int>(b => (int) b.Shell));
    }
}
