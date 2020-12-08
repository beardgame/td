using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.GameState.World
{
    struct TilePassabilityChanged : IGlobalEvent
    {
        public Tile Tile { get; }

        public TilePassabilityChanged(Tile tile)
        {
            Tile = tile;
        }
    }
}
