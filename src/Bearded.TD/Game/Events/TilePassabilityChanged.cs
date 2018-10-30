
using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TilePassabilityChanged : IEvent
    {
        public Tile<TileInfo> Tile { get; }

        public TilePassabilityChanged(Tile<TileInfo> tile)
        {
            Tile = tile;
        }
    }
}
