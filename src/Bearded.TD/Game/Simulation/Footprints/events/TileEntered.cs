using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Footprints;

readonly record struct TileEntered(Tile Tile) : IComponentEvent;
