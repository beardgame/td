using System;
using System.Linq;
using Bearded.TD.Game.Tiles;
using Bearded.TD.Rendering;
using Bearded.Utilities.SpaceTime;
using OpenTK;
using static System.Math;
using static Bearded.TD.Constants.Game.World;

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

    static class TileMath
    {
        public static (Direction direction, Position2 point) GetOutIntersection(Ray ray)
        {
            
            var rayStart = ray.Start.NumericValue;
            var rayDirection = ray.Direction.NumericValue;

            foreach (var dir in Tiles.Extensions.Directions)
            {
                var v = dir.CornerAfter() * HexagonSide;
                var v2 = dir.CornerBefore() * HexagonSide - v;

                var denominator = rayDirection.Y * v2.X - rayDirection.X * v2.Y;

                // disregard back facing and parallel walls
                // (denominator is dot product of non-unit normal and ray)
                if (denominator <= 0)
                    continue;

                var numerator = (v.Y - rayStart.Y) * v2.X + (rayStart.X - v.X) * v2.Y;

                var f = numerator / denominator;

                // disregard behind result
                if (f < -0.001f)
                    continue;

                var point = rayStart + f * rayDirection;
                
                var wF = Abs(v2.X) > HexagonSide * 0.001f
                    ? (point.X - v.X) / v2.X
                    : (point.Y - v.Y) / v2.Y;

                // disregard outside of line segment
                if (wF < 0 || wF > 1)
                    continue;

                return (dir, new Position2(point));
            }

            throw new Exception("No direction found");
        }
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
        
        public TiledRayHitResult<TTileInfo> ShootRay(Ray ray)
        {
            return ShootRay(ray, GetTile(ray.Start));
        }

        public TiledRayHitResult<TTileInfo> ShootRay(Ray ray, Tile<TTileInfo> startTile)
        {
#if DEBUG
            if (GetTile(ray.Start) != startTile)
                throw new ArgumentException("Ray must start on given start tile.");
#endif

            var tile = startTile;
            var endTile = GetTile(ray.Start + ray.Direction);

            var tileCenter = new Difference2(GetPosition(tile).NumericValue);

            while (true)
            {
                var rayLocal = new Ray(ray.Start - tileCenter, ray.Direction); // direction is not correct length?

                // TODO: try hitting things on this tile

                if (tile == endTile)
                    return RayHitResult.Miss(ray).OnTile(tile, new Difference2());



                var (outDirection, outPoint) = TileMath.GetOutIntersection(rayLocal);
                var outVector = outDirection.Vector();
                tile = tile.Neighbour(outDirection);

                if (TileBlocksRays(tile))
                    return RayHitResult
                        .Hit(ray, outPoint + tileCenter, new Difference2(-outVector))
                        .OnTile(tile, new Difference2());

                tileCenter += new Difference2(outVector * HexagonWidth);
            }
        }

        protected virtual bool TileBlocksRays(Tile<TTileInfo> tile) => false;
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

        protected override bool TileBlocksRays(Tile<TileInfo> tile)
            => !tile.IsValid || tile.Info.TileType == TileInfo.Type.Wall;
    }
}
