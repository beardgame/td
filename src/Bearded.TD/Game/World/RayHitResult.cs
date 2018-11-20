using Bearded.TD.Tiles;
using Bearded.TD.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
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
}