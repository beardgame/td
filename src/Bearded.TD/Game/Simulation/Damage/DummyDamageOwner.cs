using Bearded.TD.Game.Simulation.Factions;

namespace Bearded.TD.Game.Simulation.Damage
{
    class DummyDamageOwner : IDamageOwner
    {
        public Faction Faction { get; }

        public DummyDamageOwner(Faction faction)
        {
            Faction = faction;
        }
    }
}
