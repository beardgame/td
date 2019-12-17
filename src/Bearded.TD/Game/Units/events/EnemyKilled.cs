using Bearded.TD.Game.Events;
using Bearded.TD.Game.Factions;

namespace Bearded.TD.Game.Units
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
