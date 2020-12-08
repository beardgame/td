using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.GameState.World
{
    struct TileDrawInfoChanged : IGlobalEvent
    {
        public Tile Tile { get; }

        public TileDrawInfoChanged(Tile tile) => Tile = tile;
    }
}
