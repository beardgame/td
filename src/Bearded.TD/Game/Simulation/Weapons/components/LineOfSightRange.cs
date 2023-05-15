using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Game.Simulation.World;
using Bearded.TD.Tiles;
using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

sealed class LineOfSightRanger : IRanger
{
    public ImmutableArray<Tile> GetTilesInRange(
        GameState game,
        PassabilityLayer passabilityLayer,
        IWeaponState weapon,
        Unit minimumRange,
        Unit maximumRange)
    {
        var position = weapon.Position.XY();
        var rangeSquared = maximumRange.Squared;
        var minRangeSquared = minimumRange.Squared;

        var level = game.Level;

        var visibilityChecker = weapon.MaximumTurningAngle is { } maxAngle
            ? new LevelVisibilityChecker().InDirection(weapon.NeutralDirection, maxAngle)
            : new LevelVisibilityChecker();

        return visibilityChecker.EnumerateVisibleTiles(
                level,
                position,
                maximumRange,
                t => !level.IsValid(t) || !passabilityLayer[t].IsPassable)
            .Where(t => !t.visibility.IsBlocking && t.visibility.VisiblePercentage > 0.2 &&
                (Level.GetPosition(t.tile) - position).LengthSquared is var dSquared &&
                dSquared >= minRangeSquared && dSquared <= rangeSquared)
            .Select(t => t.tile)
            .ToImmutableArray();
    }
}
