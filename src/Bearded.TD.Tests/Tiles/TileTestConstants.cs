using System.Collections.Generic;
using Bearded.TD.Tiles;

namespace Bearded.TD.Tests.Tiles
{
    static class TileTestConstants
    {
        public static IEnumerable<Tile> SomeArbitraryTiles { get; } =
            new List<Tile>
            {
                new (0, 0),
                new (1, 0),
                new (0, 1),
                new (1, 1),
                new (-1, 0),
                new (0, -1),
                new (-1, -1),
                new (1, -1),
                new (-1, 1),
                new (24, 0),
                new (0, 37),
                new (42, 11678),
                new (-215, 0),
                new (0, -23),
                new (-125, -16162),
                new (2341, -1123),
                new (-2315, 2673),
                new (10000, 10000),
            };
    }
}
