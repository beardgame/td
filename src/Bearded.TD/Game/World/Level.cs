﻿using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using static System.Math;
using static Bearded.TD.Constants.Game.World;

namespace Bearded.TD.Game.World
{
    class Level<TTileInfo>
    {
        public Tilemap<TTileInfo> Tilemap { get; }

        public Level(Tilemap<TTileInfo> tilemap)
        {
            Tilemap = tilemap;
        }

        public Tile<TTileInfo> GetTile(Position2 position)
        {
            var yf = position.Y.NumericValue * (1 / HexagonDistanceY) + 1 / 1.5f;
            var y = Floor(yf);
            var xf = position.X.NumericValue * (1 / HexagonWidth) - y * 0.5f + 0.5f;
            var x = Floor(xf);

            var xRemainder = (xf - x) - 0.5f;
            var yRemainder = (yf - y) * 1.5f;

            var tx = (int) x;
            var ty = (int) y;

            if (xRemainder > yRemainder)
            {
                tx++;
                ty--;
            }
            else if (-xRemainder > yRemainder)
                ty--;

            return new Tile<TTileInfo>(Tilemap, tx, ty);
        }

        public Position2 GetPosition(Tile<TTileInfo> tile)
            => new Position2(
                (tile.X + tile.Y * 0.5f) * HexagonDistanceX,
                tile.Y * HexagonDistanceY
            );
    }

    sealed class Level : Level<TileInfo>
    {
        public Level(Tilemap<TileInfo> tilemap)
            : base(tilemap)
        {
        }

        public void Draw(GeometryManager geos)
        {
            var geo = geos.Level;

            foreach (var tile in Tilemap)
            {
                var info = tile.Info;

                geo.DrawTile(
                    GetPosition(tile).NumericValue,
                    info.TileType == TileInfo.Type.Floor,
                    info.OpenDirections
                    );
            }

            return;


            var sprite = geos.Sprites["hex"];
            
            sprite.Size = new Vector2(HexagonDiameter, HexagonDiameter);
            
            foreach (var tile in Tilemap)
            {
                var p = GetPosition(tile).NumericValue;

                sprite.Color = Tilemap[tile].IsPassable ? Color.Gray : Color.DarkSlateGray;

                sprite.DrawSprite(new Vector2(p.X, p.Y));
            }
        }
    }
}
