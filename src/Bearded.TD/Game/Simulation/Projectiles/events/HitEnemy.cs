using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.Units;

namespace Bearded.TD.Game.Simulation.Projectiles
{
    readonly struct HitEnemy : IComponentEvent
    {
        public EnemyUnit Enemy { get; }

        public HitEnemy(EnemyUnit enemy)
        {
            Enemy = enemy;
        }
    }
}
