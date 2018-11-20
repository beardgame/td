using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Events
{
    struct TileTypeChanged : IEvent
    {
        public Tile Tile { get; }

        public TileTypeChanged(Tile tile) => Tile = tile;
    }
}