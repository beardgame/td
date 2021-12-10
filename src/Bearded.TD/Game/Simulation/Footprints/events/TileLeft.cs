using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints
{
    readonly record struct TileLeft(Tile Tile) : IComponentEvent;
}
