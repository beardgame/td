using Bearded.TD.Game.World;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TileDrawInfoChanged : IEvent
    {
        public Tile<TileInfo> Tile { get; }

        public TileDrawInfoChanged(Tile<TileInfo> tile)
        {
            Tile = tile;
        }
    }
}
