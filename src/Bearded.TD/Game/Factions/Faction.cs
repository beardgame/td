using Bearded.TD.Utilities;

namespace Bearded.TD.Game.Factions
{
    class Faction : IIdable<Faction>
    {
        public Id<Faction> Id { get; }

        public Faction(Id<Faction> id, Faction parent)
        {
            Id = id;
        }
    }
}
