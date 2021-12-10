using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Rules.GameLoop;

[GameRule("deferWaveUntilTechPointUsed")]
sealed class DeferWaveUntilTechPointUsed : GameRule<DeferWaveUntilTechPointUsed.RuleParameters> {

    public DeferWaveUntilTechPointUsed(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        if (!context.Factions.Find(Parameters.Faction).TryGetBehavior<FactionTechnology>(out var technology))
        {
            context.Logger.Warning?.Log("Pause until tech point used rule defined for faction without technology.");
            return;
        }
        context.Events.Subscribe(new Listener(technology, context.Blueprints.Technologies.All));
    }

    private sealed class Listener : IListener<WaveScheduled>
    {
        private readonly FactionTechnology factionTechnology;
        private readonly IEnumerable<ITechnologyBlueprint> technologyBlueprints;

        public Listener(FactionTechnology factionTechnology, IEnumerable<ITechnologyBlueprint> technologyBlueprints)
        {
            this.factionTechnology = factionTechnology;
            this.technologyBlueprints = technologyBlueprints.ToImmutableArray();
        }

        public void HandleEvent(WaveScheduled @event)
        {
            if (technologyBlueprints.Any(factionTechnology.CanUnlockTechnology))
            {
                @event.SpawnStartRequirementConsumer(new TechPointSpawnStartRequirement(factionTechnology));
            }
        }
    }

    private sealed class TechPointSpawnStartRequirement : ISpawnStartRequirement
    {
        private readonly FactionTechnology factionTechnology;

        public bool Satisfied => factionTechnology.TechPoints == 0;

        public TechPointSpawnStartRequirement(FactionTechnology factionTechnology)
        {
            this.factionTechnology = factionTechnology;
        }
    }

    public sealed record RuleParameters(ExternalId<Faction> Faction);
}