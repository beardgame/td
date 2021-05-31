using System.Collections.Generic;
using System.Collections.Immutable;

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
}
