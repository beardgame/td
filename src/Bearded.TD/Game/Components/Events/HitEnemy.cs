using Bearded.TD.Game.Units;

namespace Bearded.TD.Game.Components.Events
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
