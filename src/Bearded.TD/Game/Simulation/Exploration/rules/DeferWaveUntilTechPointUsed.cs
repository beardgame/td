using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Exploration;

[GameRule("deferWaveUntilExplorationTokenUsed")]
sealed class DeferWaveUntilExplorationTokenUsed : GameRule<DeferWaveUntilExplorationTokenUsed.RuleParameters> {

    public DeferWaveUntilExplorationTokenUsed(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        var faction = context.Factions.Find(Parameters.Faction);
        if (!faction.TryGetBehavior<FactionExploration>(out var factionExploration))
        {
            context.Logger.Warning?.Log(
                $"Cannot apply defer wave until exploration token use to faction {faction} because it does not " +
                "support exploration.");
            return;
        }
        context.Events.Subscribe(new Listener(factionExploration));
    }

    private sealed class Listener : IListener<WaveScheduled>
    {
        private readonly FactionExploration factionExploration;

        public Listener(FactionExploration factionExploration)
        {
            this.factionExploration = factionExploration;
        }

        public void HandleEvent(WaveScheduled @event)
        {
            if (factionExploration.ExplorableZones.Length > 0)
            {
                @event.SpawnStartRequirementConsumer(new ExplorationTokenSpawnStartRequirement(factionExploration));
            }
        }
    }

    private sealed class ExplorationTokenSpawnStartRequirement : ISpawnStartRequirement
    {
        private readonly FactionExploration factionExploration;

        public bool Satisfied => !factionExploration.HasExplorationToken;

        public ExplorationTokenSpawnStartRequirement(FactionExploration factionExploration)
        {
            this.factionExploration = factionExploration;
        }
    }

    public sealed record RuleParameters(ExternalId<Faction> Faction);
}
