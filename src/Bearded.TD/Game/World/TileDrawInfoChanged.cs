using Bearded.TD.Game.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.World
{
    struct TileDrawInfoChanged : IGlobalEvent
    {
        public Tile Tile { get; }

        public TileDrawInfoChanged(Tile tile) => Tile = tile;
    }
}
