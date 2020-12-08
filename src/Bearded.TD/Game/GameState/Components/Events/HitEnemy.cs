using Bearded.TD.Game.GameState.Units;

namespace Bearded.TD.Game.GameState.Components.Events
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
