using Bearded.TD.Game.Simulation.Components;

namespace Bearded.TD.Game.Simulation.Projectiles;

readonly record struct HitEnemy(ComponentGameObject Enemy, HitInfo Info) : IComponentEvent;
