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

            public IEnumerator<Tile> GetEnumerator() => tiles.GetEnumerator();
        }
    }
}
