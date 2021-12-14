using System.Collections.ObjectModel;
using Bearded.TD.Content.Mods;
using Bearded.Utilities;

namespace Bearded.TD.Game.Simulation.Factions;

interface IGameFactions
{
    ReadOnlyCollection<Faction> All { get; }
    Faction Resolve(Id<Faction> id);
    Faction Find(ExternalId<Faction> externalId);
}