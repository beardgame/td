using Bearded.TD.Game.Units;

namespace Bearded.TD.Game.Components.Events
{
    struct HitEnemy : IEvent
    {
        public EnemyUnit Enemy { get; }

        public HitEnemy(EnemyUnit enemy)
        {
            Enemy = enemy;
        }
    }
}
