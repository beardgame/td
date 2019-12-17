using Bearded.TD.Game.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.World
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
