using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Physics;

readonly record struct CollideWithLevel(Impact Info, Tile Tile) : IComponentEvent;
