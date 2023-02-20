using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Projectiles;

readonly record struct HitEnemy(GameObject Enemy, Impact Impact) : IComponentEvent;
