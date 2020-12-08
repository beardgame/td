using Bearded.TD.Game.GameState.Events;
using Bearded.TD.Game.GameState.Factions;

namespace Bearded.TD.Game.GameState.Units
{
    struct EnemyKilled : IGlobalEvent
    {
        public EnemyUnit Unit { get; }
        public Faction KillingFaction { get; }

        public EnemyKilled(EnemyUnit unit, Faction killingFaction)
        {
            Unit = unit;
            KillingFaction = killingFaction;
        }
    }
}
