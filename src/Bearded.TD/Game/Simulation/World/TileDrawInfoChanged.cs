using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.World
{
    struct TileDrawInfoChanged : IGlobalEvent
    {
        public Tile Tile { get; }

        public TileDrawInfoChanged(Tile tile) => Tile = tile;
    }
}
