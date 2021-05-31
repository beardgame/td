using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bearded.TD.Tiles
{
    interface IArea
    {
        bool Contains(Tile tile);

        IEnumerable<Tile> Enumerated { get; }
        IEnumerator<Tile> GetEnumerator();
        ImmutableHashSet<Tile> ToImmutableHashSet();
        ImmutableArray<Tile> ToImmutableArray();
    }

    static class Area
    {
        public static IArea From(IEnumerable<Tile> tiles)
        {
            return tiles switch
            {
                ImmutableHashSet<Tile> hashSet => new HashSetArea(hashSet),
                _ => new HashSetArea(tiles.ToImmutableHashSet())
            };
        }

        private sealed class HashSetArea : IArea
        {
            private readonly ImmutableHashSet<Tile> tiles;

            public HashSetArea(ImmutableHashSet<Tile> tiles)
            {
                this.tiles = tiles;
            }

            public bool Contains(Tile tile) => tiles.Contains(tile);

            public IEnumerable<Tile> Enumerated => tiles;
            public IEnumerator<Tile> GetEnumerator() => tiles.GetEnumerator();
            public ImmutableHashSet<Tile> ToImmutableHashSet() => tiles;
            public ImmutableArray<Tile> ToImmutableArray() => tiles.ToImmutableArray();
        }

        public static IArea Erode(this IArea area) =>
            From(area.Enumerated.Where(t => t.PossibleNeighbours().All(area.Contains)));
    }
}
