using System.Collections.Generic;
using System.Collections.Immutable;
using Bearded.TD.Game.Simulation.Navigation;
using Bearded.TD.Tiles;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Simulation.Weapons;

sealed class FloodFillRanger : IRanger
{
    private readonly Queue<(Tile Tile, int Steps)> toExpand = new();
    private readonly HashSet<Tile> seen = new();
    private readonly List<Tile> result = new();

    public ImmutableArray<Tile> GetTilesInRange(
        GameState game,
        PassabilityLayer passabilityLayer,
        IEnumerable<Tile> origin,
        Unit minimumRange,
        Unit maximumRange)
    {
        var level = game.Level;

        var minSteps = MathExtensions.CeiledToInt(minimumRange.NumericValue);
        var maxSteps = MathExtensions.FlooredToInt(maximumRange.NumericValue);

        seen.Clear();
        toExpand.Clear();
        result.Clear();

        foreach (var o in origin)
        {
            seen.Add(o);
            toExpand.Enqueue((o, 0));
        }

        while (toExpand.Count > 0)
        {
            var (tile, steps) = toExpand.Dequeue();

            if (!passabilityLayer[tile].IsPassable)
                continue;

            if (steps >= minSteps)
                result.Add(tile);

            if (steps == maxSteps)
                continue;

            steps++;

            foreach (var neighbor in tile.PossibleNeighbours())
            {
                if (level.IsValid(tile) && seen.Add(neighbor))
                    toExpand.Enqueue((neighbor, steps));
            }
        }

        return result.ToImmutableArray();
    }
}
