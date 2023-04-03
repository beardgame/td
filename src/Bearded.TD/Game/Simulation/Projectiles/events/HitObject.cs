using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Projectiles;

readonly record struct HitObject(GameObject Object, Impact Impact) : IComponentEvent;
