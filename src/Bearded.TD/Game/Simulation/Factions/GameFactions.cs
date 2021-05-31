using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Content.Mods;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Factions
{
    sealed class GameFactions
    {
        private readonly IdCollection<Faction> factions = new();
        private readonly Dictionary<ExternalId<Faction>, Faction> factionsByExternalId = new();

        public ReadOnlyCollection<Faction> All { get; }

        public GameFactions()
        {
            All = factions.AsReadOnly;
        }

        public void Add(Faction faction)
        {
            factions.Add(faction);
            if (faction.ExternalId != ExternalId<Faction>.Invalid)
            {
                factionsByExternalId.Add(faction.ExternalId, faction);
            }
        }

        public Faction Resolve(Id<Faction> id) => factions[id];

        public Faction Find(ExternalId<Faction> externalId) => factionsByExternalId[externalId];
    }
}
