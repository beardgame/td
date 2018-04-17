using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bearded.TD.Utilities;

namespace Bearded.TD.Tiles
{
    static class Tilemap
    {
        public static ReadOnlyCollection<Direction> Directions { get; }
            = Extensions.Directions;

        public static int TileCountForRadius(int radius) => 3 * radius * (radius + 1) + 1;
    }

    class Tilemap<TTileInfo> : IEnumerable<Tile<TTileInfo>>
    {
        public int Radius { get; }
        private readonly TTileInfo[,] tiles;

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

        public Tilemap(int radius, Func<Tile<TTileInfo>, TTileInfo> initialiseTile)
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
            tiles = new TTileInfo[arrayDimension, arrayDimension];
        }

        public int Count => Tilemap.TileCountForRadius(Radius);

        public TTileInfo this[int x, int y]
        {
            get { return tiles[x + Radius, y + Radius]; }
            set { tiles[x + Radius, y + Radius] = value; }
        }

        public TTileInfo this[Tile<TTileInfo> tile]
        {
            get { return this[tile.X, tile.Y]; }
            set { this[tile.X, tile.Y] = value; }
        }

        public bool IsValidTile(int x, int y) =>
            Math.Abs(x) <= Radius &&
            Math.Abs(y) <= Radius &&
            Math.Abs(x + y) <= Radius;

        public bool IsValidTile(Tile<TTileInfo> tile) => IsValidTile(tile.X, tile.Y);

        public IEnumerable<Tile<TTileInfo>> SpiralCenteredAt(Tile<TTileInfo> center, int radius)
            => spiralTiles(center.X, center.Y, radius).Where(t => t.IsValid);

        public IEnumerable<Tile<TTileInfo>> TilesSpiralOutward => spiralTiles(0, 0, Radius);

        private IEnumerable<Tile<TTileInfo>> spiralTiles(int centerX, int centerY, int radius)
            => spiral(centerX, centerY, radius).Select(xy => new Tile<TTileInfo>(this, xy.X, xy.Y));

        private IEnumerable<Vector2i> spiral(int centerX, int centerY, int radius)
        {
            var x = 0;
            var y = 0;

            yield return new Vector2i(centerX, centerY);

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
                        yield return new Vector2i(centerX + x, centerY + y);

                        x += step.X;
                        y += step.Y;
                    }
                }

            }
        }

        public IEnumerator<Tile<TTileInfo>> GetEnumerator()
        {
            for (var y = -Radius; y <= Radius; y++)
            {
                var xMin = Math.Max(-Radius, -Radius - y);
                var xMax = Math.Min(Radius, Radius - y);

                for (var x = xMin; x <= xMax; x++)
                {
                    yield return new Tile<TTileInfo>(this, x, y);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    }
}
