using System.Collections.Immutable;

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
