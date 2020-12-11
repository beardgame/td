using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Units
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
