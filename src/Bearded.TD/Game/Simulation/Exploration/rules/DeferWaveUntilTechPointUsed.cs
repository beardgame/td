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
        // TODO: use the faction from the parameters
        context.Events.Subscribe(new Listener(context.GameState.ExplorationManager));
    }

    private sealed class Listener : IListener<WaveScheduled>
    {
        private readonly ExplorationManager explorationManager;

        public Listener(ExplorationManager explorationManager)
        {
            this.explorationManager = explorationManager;
        }

        public void HandleEvent(WaveScheduled @event)
        {
            if (explorationManager.ExplorableZones.Length > 0)
            {
                @event.SpawnStartRequirementConsumer(new ExplorationTokenSpawnStartRequirement(explorationManager));
            }
        }
    }

    private sealed class ExplorationTokenSpawnStartRequirement : ISpawnStartRequirement
    {
        private readonly ExplorationManager explorationManager;

        public bool Satisfied => !explorationManager.HasExplorationToken;

        public ExplorationTokenSpawnStartRequirement(ExplorationManager explorationManager)
        {
            this.explorationManager = explorationManager;
        }
    }

    public sealed record RuleParameters(ExternalId<Faction> Faction);
}