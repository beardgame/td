using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
    struct TiledRayHitResult
    {
        public Tile Tile { get; }
        public Position2 GlobalPoint { get; }
        public RayHitResult Results { get; }

        public TiledRayHitResult(Tile tile, RayHitResult results, Difference2 tileOffset)
        {
            Tile = tile;
            GlobalPoint = results.Point + tileOffset;
            Results = results;
        }
    }
}
