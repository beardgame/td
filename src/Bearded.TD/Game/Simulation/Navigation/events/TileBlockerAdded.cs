using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Navigation;

readonly record struct TileBlockerAdded(Tile Tile) : IGlobalEvent;
