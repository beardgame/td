using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Simulation.Weapons;

sealed partial class TargetEnemiesInRange
{
    private void ensureTilesInRangeUpToDate()
    {
        var rangeChanged = currentRange != Parameters.Range
            || !currentMaxTurningAngle.Equals(weapon.MaximumTurningAngle);

        if (rangeChanged || Owner.Game.Time >= nextTileInRangeRecalculationTime)
        {
            recalculateTilesInRange();
        }
    }

    private void recalculateTilesInRange()
    {
        currentMaxTurningAngle = weapon.MaximumTurningAngle;
        currentRange = Parameters.Range;
        var rangeSquared = currentRange.Squared;
        var minRangeSquared = Parameters.MinimumRange.Squared;

        var level = Owner.Game.Level;
        var navigator = Owner.Game.Navigator;

        var visibilityChecker = currentMaxTurningAngle is { } maxAngle
            ? new LevelVisibilityChecker().InDirection(weapon.NeutralDirection, maxAngle)
            : new LevelVisibilityChecker();

        tilesInRange = visibilityChecker.EnumerateVisibleTiles(
                level,
                Owner.Position.XY(),
                currentRange,
                t => !level.IsValid(t) || !passabilityLayer[t].IsPassable)
            .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                (Level.GetPosition(t.tile) - Owner.Position.XY()).LengthSquared is var dSquared &&
                dSquared >= minRangeSquared && dSquared <= rangeSquared)
            .Select(t => t.tile)
            .OrderBy(navigator.GetDistanceToClosestSink)
            .ToImmutableArray();

        nextTileInRangeRecalculationTime = Owner.Game.Time + Parameters.ReCalculateTilesInRangeInterval;
    }
}
