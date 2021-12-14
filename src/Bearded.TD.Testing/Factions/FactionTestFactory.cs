using Bearded.TD.Content.Models;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Factions;
using static Bearded.TD.Testing.UniqueIds;

namespace Bearded.TD.Testing.Factions;

static class FactionTestFactory
{
    public static Faction CreateFaction() => CreateFaction(new GlobalGameEvents());

    public static Faction CreateFaction(GlobalGameEvents events) => Faction.FromBlueprint(
        NextUniqueId<Faction>(),
        null,
        new FactionBlueprint(
            NextUniqueExternalId<Faction>("faction"), null, null, Enumerable.Empty<IFactionBehaviorFactory<Faction>>()),
        events);
}
