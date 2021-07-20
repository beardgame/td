using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints
{
    readonly struct TileLeft : IComponentEvent
    {
        public Tile Tile { get; }

        public TileLeft(Tile tile)
        {
            Tile = tile;
        }
    }
}
