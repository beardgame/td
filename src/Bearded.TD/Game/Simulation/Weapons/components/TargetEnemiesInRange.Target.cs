using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Weapons;

sealed partial class TargetEnemiesInRange
{
    private void ensureTargetValid()
    {
        if (target?.Deleted == true)
        {
            target = null;
        }

        // TODO: accumulating tiles each frame is expensive, can we somehow cache this?
        if (target != null && !tilesInRange.OverlapsWithTiles(OccupiedTileAccumulator.AccumulateOccupiedTiles(target)))
        {
            target = null;
        }

        if (target == null)
        {
            tryFindTarget();
        }
    }

    private void tryFindTarget()
    {
        target = targetingMode.SelectTarget(tilesInRange.SelectMany(Owner.Game.UnitLayer.GetUnitsOnTile));
    }
}
