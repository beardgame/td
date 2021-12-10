using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Units;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    readonly record struct HitEnemy(EnemyUnit Enemy) : IComponentEvent;
}
