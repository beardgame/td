using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bearded.TD.Tiles
{
    static partial class Area
    {
        public static IArea From(IEnumerable<Tile> tiles)
        {
            return tiles switch
            {
                ImmutableHashSet<Tile> hashSet => new HashSetArea(hashSet),
                _ => new HashSetArea(tiles.ToImmutableHashSet())
            };
        }

        public static IArea Erode(this IArea area) =>
            From(area.Enumerated.Where(t => t.PossibleNeighbours().All(area.Contains)));
    }
}
