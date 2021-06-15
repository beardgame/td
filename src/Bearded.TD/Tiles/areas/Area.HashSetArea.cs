using System.Collections.Generic;
using System.Collections.Immutable;

namespace Bearded.TD.Tiles
{
    static partial class Area
    {
        private sealed class HashSetArea : IArea
        {
            private readonly ImmutableHashSet<Tile> tiles;

            public HashSetArea(ImmutableHashSet<Tile> tiles)
            {
                this.tiles = tiles;
            }

            public int Count => tiles.Count;
            public bool Contains(Tile tile) => tiles.Contains(tile);

            public IEnumerable<Tile> Enumerated => tiles;
            public IEnumerator<Tile> GetEnumerator() => tiles.GetEnumerator();
            public ImmutableHashSet<Tile> ToImmutableHashSet() => tiles;
            public ImmutableArray<Tile> ToImmutableArray() => tiles.ToImmutableArray();
        }
    }
}
