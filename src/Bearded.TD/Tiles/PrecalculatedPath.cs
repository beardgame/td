using System.Collections.Generic;
using System.Collections.Immutable;
using static Bearded.TD.Utilities.DebugAssert;

namespace Bearded.TD.Tiles;

sealed class PrecalculatedPath
{
    public Tile StartTile { get; }
    public Tile GoalTile { get; }

    private readonly ImmutableDictionary<Tile, Direction> tileDirectionLookup;

    private PrecalculatedPath(Tile startTile, Tile goalTile, ImmutableDictionary<Tile, Direction> tileDirectionLookup)
    {
        StartTile = startTile;
        GoalTile = goalTile;
        this.tileDirectionLookup = tileDirectionLookup;
    }

    public Direction NextDirectionFromTile(Tile t)
    {
        return tileDirectionLookup.GetValueOrDefault(t, Direction.Unknown);
    }

    public ImmutableArray<Tile> ToTileList()
    {
        return toTileEnumerable().ToImmutableArray();
    }

    private IEnumerable<Tile> toTileEnumerable()
    {
        yield return StartTile;
        var current = StartTile;
        while (tileDirectionLookup.TryGetValue(current, out var dir))
        {
            current = current.Neighbor(dir);
            yield return current;
        }
        State.Satisfies(current == GoalTile);
    }

    public static PrecalculatedPath FromPathfindingResult(Tile startTile, Pathfinder.Result result)
    {
        var currentTile = startTile;
        var dict = ImmutableDictionary.CreateBuilder<Tile, Direction>();
        foreach (var dir in result.Path)
        {
            dict.Add(currentTile, dir);
            currentTile = currentTile.Neighbor(dir);
        }

        return new PrecalculatedPath(startTile, currentTile, dict.ToImmutable());
    }
}
