using System;
using System.Linq;
using System.Text;
using amulware.Graphics;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.Utilities;
using Bearded.Utilities.Linq;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using static System.Math;
using static Bearded.TD.Constants.Game.World;
using Extensions = Bearded.TD.Game.Tiles.Extensions;

namespace Bearded.TD.Game.World
{
    struct Ray
    {
        public Position2 Start { get; }
        public Difference2 Direction { get; }

        public Ray(Position2 start, Difference2 direction)
        {
            Start = start;
            Direction = direction;
        }
    }

    struct TiledRayHitResult<TTileInfo>
    {
        public Tile<TTileInfo> Tile { get; }
        public Position2 GlobalPoint { get; }
        public RayHitResult Results { get; }

        public TiledRayHitResult(Tile<TTileInfo> tile, RayHitResult results, Difference2 tileOffset)
        {
            Tile = tile;
            GlobalPoint = results.Point + tileOffset;
            Results = results;
        }

    }

    struct RayHitResult
    {
        public bool IsHit { get; }
        public float RayFactor { get; }
        public Position2 Point { get; }
        public Difference2 Normal { get; }


        public static RayHitResult Hit(Ray ray, Position2 hitPoint, Difference2 normal = default(Difference2))
            => new RayHitResult(true, ray, hitPoint, normal);

        public static RayHitResult Hit(Ray ray, float rayFactor, Difference2 normal = default(Difference2))
            => new RayHitResult(true, rayFactor, ray.Start + rayFactor * ray.Direction, normal);

        public static RayHitResult Miss(Ray ray)
            => new RayHitResult(false, 1, ray.Start + ray.Direction, default(Difference2));

        private RayHitResult(bool isHit, Ray ray, Position2 point, Difference2 normal)
        {
            IsHit = isHit;

            RayFactor = ray.Direction.X != 0.U()
                ? (point.X - ray.Start.X) / ray.Direction.X
                : (point.Y - ray.Start.Y) / ray.Direction.Y;

            Point = point;
            Normal = normal;
        }

        private RayHitResult(bool isHit, float rayFactor, Position2 point, Difference2 normal)
        {
            IsHit = isHit;
            RayFactor = rayFactor;
            Point = point;
            Normal = normal;
        }

        public RayHitResult WithNewPoint(Position2 point)
            => new RayHitResult(IsHit, RayFactor, point, Normal);

        public TiledRayHitResult<TTileInfo> OnTile<TTileInfo>(Tile<TTileInfo> tile, Difference2 offset)
            => new TiledRayHitResult<TTileInfo>(tile, this, offset);
    }

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
            
            var isBottomRightCorner = xRemainder > yRemainder;
            var isBottomLeftConer = -xRemainder > yRemainder;

            tx += isBottomRightCorner ? 1 : 0;
            ty += isBottomRightCorner | isBottomLeftConer ? -1 : 0;

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
                    info.TileType,
                    tile.Neighbour(Direction.Right).ValidOrNull?.Info.TileType ?? TileInfo.Type.Unknown,
                    tile.Neighbour(Direction.UpRight).ValidOrNull?.Info.TileType ?? TileInfo.Type.Unknown,
                    tile.Neighbour(Direction.DownRight).ValidOrNull?.Info.TileType ?? TileInfo.Type.Unknown
                    );
            }
        }
    }
}
