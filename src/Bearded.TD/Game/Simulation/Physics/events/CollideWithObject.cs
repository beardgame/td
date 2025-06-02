using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Physics;

readonly record struct CollidedWithObject(GameObject Object, Impact Impact) : IComponentEvent;
