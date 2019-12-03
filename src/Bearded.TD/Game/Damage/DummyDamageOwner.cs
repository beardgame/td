using Bearded.TD.Game.Factions;

namespace Bearded.TD.Game.Damage
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
