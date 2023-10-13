using System.Collections.Generic;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Tiles;

static partial class Area
{
    private readonly record struct SingleTileArea(Tile Tile) : IArea
    {
        public int Count => 1;
        public bool Contains(Tile tile) => tile == Tile;

        public IEnumerator<Tile> GetEnumerator() => Tile.Yield().GetEnumerator();
    }
}
