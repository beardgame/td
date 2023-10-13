using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bearded.TD.Tiles;

static partial class Area
{
    public static IArea Empty() => From(ImmutableHashSet<Tile>.Empty);

    public static IArea Single(Tile tile) => new SingleTileArea(tile);

    public static IArea From(IEnumerable<Tile> tiles)
    {
        return tiles switch
        {
            ImmutableHashSet<Tile> hashSet => new HashSetArea(hashSet),
            _ => new HashSetArea(tiles.ToImmutableHashSet())
        };
    }

    public static IArea Erode(this IArea area) =>
        Erode(area, t => t.PossibleNeighbours().All(area.Contains));

    public static IArea Erode(this IArea area, int minimumNeighborsToKeepTile) =>
        Erode(area, n => n >= minimumNeighborsToKeepTile);

    public static IArea Erode(this IArea area, Func<int, bool> keepTileWithNeighborCount) =>
        Erode(area, t => keepTileWithNeighborCount(t.PossibleNeighbours().Count(area.Contains)));

    public static IArea Erode(this IArea area, Func<Tile, bool> keepTile) => From(area.Where(keepTile));
}
