using Bearded.TD.Game.Simulation.Units;

namespace Bearded.TD.Game.Simulation.Components.Events
{
    struct HitEnemy : IComponentEvent
    {
        public EnemyUnit Enemy { get; }

        public HitEnemy(EnemyUnit enemy)
        {
            Enemy = enemy;
        }
    }
}
