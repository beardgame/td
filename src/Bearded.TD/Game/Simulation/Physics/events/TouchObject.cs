using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Physics;

readonly record struct TouchObject(GameObject Object, Impact Impact) : IComponentEvent;
