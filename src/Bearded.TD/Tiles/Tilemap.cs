using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Bearded.TD.Tiles
{
    static class Tilemap
    {
        public static ReadOnlyCollection<Direction> Directions { get; }
            = Extensions.Directions;

        public static int TileCountForRadius(int radius) => 3 * radius * (radius + 1) + 1;
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
            => spiral(center.X, center.Y, radius)
                .Where(IsValidTile);

        public IEnumerable<Tile> TilesSpiralOutward
            => spiral(0, 0, Radius);

        private IEnumerable<Tile> spiral(int centerX, int centerY, int radius)
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

        public IEnumerator<Tile> GetEnumerator() => TilemapHelpers.EnumerateTilemapWith(Radius);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
