using System.Linq;
using Bearded.TD.Game.Simulation.Footprints;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Weapons;

sealed partial class TargetEnemiesInRange
{
    public void HandleEvent(TargetingModeChanged @event)
    {
        tryFindTarget();
    }

    private void ensureTargetValid()
    {
        if (hasValidTarget())
            return;

        tryFindTarget();
    }

    private bool hasValidTarget()
    {
        if (target == null)
            return false;

        if (target.Deleted)
            return false;

        if (Owner.Game.Time >= endOfKeepTargetTime)
            return false;

        if (!tilesInRange.OverlapsWithTiles(target.GetTilePresence().OccupiedTiles))
            return false;

        return true;
    }

    private void tryFindTarget()
    {
        target = weapon.TargetingMode.SelectTarget(
            tilesInRange.SelectMany(Owner.Game.TargetLayer.GetObjectsOnTile),
            new TargetingContext(Owner.Position, weapon.Direction, Owner.Game.Navigator));

        endOfKeepTargetTime = Owner.Game.Time + Parameters.RetargetInterval;
    }
}
