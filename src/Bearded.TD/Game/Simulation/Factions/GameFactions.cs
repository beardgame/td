using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bearded.TD.Content.Mods;
using Bearded.TD.Utilities.Collections;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Factions
{
    sealed class GameFactions : IGameFactions
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

        public IGameFactions AsReadOnly() => new GameFactionsProxy(this);

        private sealed class GameFactionsProxy : IGameFactions
        {
            private readonly GameFactions inner;

            public GameFactionsProxy(GameFactions inner)
            {
                this.inner = inner;
            }

            public ReadOnlyCollection<Faction> All => inner.All;

            public Faction Resolve(Id<Faction> id) => inner.Resolve(id);

            public Faction Find(ExternalId<Faction> externalId) => inner.Find(externalId);
        }
    }
}
