using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Tiles;

namespace Bearded.TD.Game.Simulation.Physics;

[Trigger("collidedWithLevel")]
readonly record struct CollidedWithLevel(Impact Info, Tile Tile) : IComponentEvent;
