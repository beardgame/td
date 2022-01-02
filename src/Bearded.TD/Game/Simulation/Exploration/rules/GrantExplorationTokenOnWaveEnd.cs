using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;

namespace Bearded.TD.Game.Simulation.Exploration;

[GameRule("grantExplorationTokenOnWaveEnd")]
sealed class GrantExplorationTokenOnWaveEnd : GameRule
{
    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(new Listener(context.GameState.ExplorationManager));
    }

    private sealed class Listener : IListener<WaveEnded>
    {
        private readonly ExplorationManager explorationManager;

        public Listener(ExplorationManager explorationManager)
        {
            this.explorationManager = explorationManager;
        }

        public void HandleEvent(WaveEnded @event)
        {
            explorationManager.AwardExplorationToken();

        }
    }
}