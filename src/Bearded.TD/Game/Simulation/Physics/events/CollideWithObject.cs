using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Physics;

readonly record struct CollideWithObject(GameObject Object, Impact Impact) : IComponentEvent;
