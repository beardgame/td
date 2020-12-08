using Bearded.TD.Game.GameState.Factions;

namespace Bearded.TD.Game.GameState.Damage
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
