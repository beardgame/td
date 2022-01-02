using System.Collections;
using System.Collections.Generic;

namespace Bearded.TD.Tiles;

interface IArea : IEnumerable<Tile>
{
    int Count { get; }

    bool Contains(Tile tile);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}