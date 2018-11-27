using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TilePassabilityChanged : IEvent
    {
        public Tile Tile { get; }

        public TilePassabilityChanged(Tile tile)
        {
            Tile = tile;
        }
    }
}
