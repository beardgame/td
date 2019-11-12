using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bearded.TD.Tiles
{
    static class Tilemap
    {
        public static ReadOnlyCollection<Direction> Directions { get; }
            = Extensions.Directions;

        public static int TileCountForRadius(int radius) => 3 * radius * (radius + 1) + 1;

        public static IEnumerable<Tile> EnumerateTilemapWith(int radius)
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

        public static IEnumerable<Tile> GetRingCenteredAt(Tile tile, int radius)
        {
            if (radius == 0)
            {
                yield return tile;
                yield break;
            }

            var (centerX, centerY) = tile;

            var x = 0;
            var y = -radius;

            // for each edge
            for (var d = 1; d <= 6; d++)
            {
                var step = ((Direction)d).Step();

                // for each tile
                for (var t = 0; t < radius; t++)
                {
                    yield return new Tile(centerX + x, centerY + y);

                    x += step.X;
                    y += step.Y;
                }
            }
        }

        public static IEnumerable<Tile> GetOutwardSpiralForTilemapWith(int radius)
            => spiral(0, 0, radius);

        public static IEnumerable<Tile> GetSpiralCenteredAt(Tile tile, int radius)
            => spiral(tile.X, tile.Y, radius);

        private static IEnumerable<Tile> spiral(int centerX, int centerY, int radius)
        {
            var x = 0;
            var y = 0;

            yield return new Tile(centerX, centerY);

            // for each circle
            for (var r = 0; r < radius; r++)
            {
                y--;

                // for each edge
                for (var d = 1; d <= 6; d++)
                {
                    var step = ((Direction)d).Step();

                    // for each tile
                    for (var t = 0; t <= r; t++)
                    {
                        yield return new Tile(centerX + x, centerY + y);

                        x += step.X;
                        y += step.Y;
                    }
                }

            }
        }
    }

    class Tilemap<TValue> : IEnumerable<Tile>
    {
        public int Radius { get; }
        private readonly TValue[,] tiles;

        /* Layout of array:
         * (radius 1)
         *
         *   /#/#/_/
         *  /#/0/#/
         * /_/#/#/
         *
         * > +x
         * ^ +y
         *
         * 0 = 0,0 origin tile
         * # = other tiles
         * _ = empty tiles (not used)
         *
         */

        public Tilemap(int radius, Func<Tile, TValue> initialiseTile)
            : this(radius)
        {
            foreach (var tile in this)
            {
                this[tile] = initialiseTile(tile);
            }
        }

        public Tilemap(int radius)
        {
            Radius = radius;
            var arrayDimension = radius * 2 + 1;
            tiles = new TValue[arrayDimension, arrayDimension];
        }

        public int Count => Tilemap.TileCountForRadius(Radius);

        public TValue this[int x, int y]
        {
            get => tiles[x + Radius, y + Radius];
            set => tiles[x + Radius, y + Radius] = value;
        }

        public TValue this[Tile tile]
        {
            get => this[tile.X, tile.Y];
            set => this[tile.X, tile.Y] = value;
        }

        public bool IsValidTile(int x, int y) =>
            Math.Abs(x) <= Radius &&
            Math.Abs(y) <= Radius &&
            Math.Abs(x + y) <= Radius;

        public bool IsValidTile(Tile tile) => IsValidTile(tile.X, tile.Y);

        public IEnumerable<Tile> SpiralCenteredAt(Tile center, int radius)
            => Tilemap.GetSpiralCenteredAt(center, radius).Where(IsValidTile);

        public IEnumerable<Tile> TilesSpiralOutward
            => Tilemap.GetOutwardSpiralForTilemapWith(Radius);

        public IEnumerator<Tile> GetEnumerator() => Tilemap.EnumerateTilemapWith(Radius).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
