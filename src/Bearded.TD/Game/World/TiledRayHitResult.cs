using Bearded.TD.Tiles;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.World
{
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
}