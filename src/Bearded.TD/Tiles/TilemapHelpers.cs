using System;
using System.Collections.Generic;

namespace Bearded.TD.Tiles
{
    static class TilemapHelpers
    {
        public static IEnumerator<Tile> EnumerateTilemapWith(int radius)
        {
            for (var y = -radius; y <= radius; y++)
            {
                var xMin = Math.Max(-radius, -radius - y);
                var xMax = Math.Min(radius, radius - y);

                for (var x = xMin; x <= xMax; x++)
                {
                    yield return new Tile(x, y);
                }
            }
        }
    }
}
