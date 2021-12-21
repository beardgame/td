using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.Linq;

namespace Bearded.TD.Game.Simulation.Exploration;

[GameRule("revealFullMap")]
sealed class RevealFullMap : GameRule<RevealFullMap.RuleParameters>
{
    public RevealFullMap(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        var factions = Parameters.Faction.HasValue
            ? ImmutableArray.Create(context.Factions.Find(Parameters.Faction.Value))
            : context.Factions.All.ToImmutableArray();
        var factionVisibilities = factions.Select(f =>
        {
            f.TryGetBehavior<FactionVisibility>(out var visibility);
            return visibility;
        }).NotNull().ToImmutableArray();

        if (!factionVisibilities.Any())
        {
            var joinedFactions = string.Join(", ", factions.Select(f => f.ExternalId));
            context.Logger.Warning?.Log(
                $"Cannot reveal full map for any specified faction because it did not support visibility. " +
                $"Factions under consideration: {joinedFactions}");
            return;
        }

        context.Events.Subscribe(new Listener(factionVisibilities));
    }

    private sealed class Listener : IListener<GameStarted>
    {
        private readonly IEnumerable<FactionVisibility> factionVisibilities;

        public Listener(IEnumerable<FactionVisibility> factionVisibilities)
        {
            this.factionVisibilities = factionVisibilities;
        }

        public void HandleEvent(GameStarted @event)
        {
            foreach (var factionVisibility in factionVisibilities)
            {
                factionVisibility.RevealAllZones();
            }
        }
    }

    public sealed record RuleParameters(ExternalId<Faction>? Faction);
}
