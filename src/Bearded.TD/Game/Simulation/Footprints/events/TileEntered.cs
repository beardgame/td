using Bearded.TD.Game.Simulation.Components.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints.events
{
    readonly struct TileEntered : IComponentEvent
    {
        public Tile Tile { get; }

        public TileEntered(Tile tile)
        {
            Tile = tile;
        }
    }
}
