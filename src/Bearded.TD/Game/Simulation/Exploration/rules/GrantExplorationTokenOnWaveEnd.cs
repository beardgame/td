using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Simulation.Exploration;

[GameRule("grantExplorationTokenOnWaveEnd")]
sealed class GrantExplorationTokenOnWaveEnd : GameRule
{
    public override void Execute(GameRuleContext context)
    {
        context.Events.Subscribe(new Listener(context.Logger));
    }

    private sealed class Listener : IListener<WaveEnded>
    {
        private readonly Logger logger;

        public Listener(Logger logger)
        {
            this.logger = logger;
        }

        public void HandleEvent(WaveEnded @event)
        {
            if (@event.TargetFaction.TryGetBehaviorIncludingAncestors<FactionExploration>(out var exploration))
            {
                exploration.AwardExplorationToken();
            }
            else
            {
                logger.Debug?.Log(
                    $"Tried awarding exploration token after wave end to {@event.TargetFaction.ExternalId}, " +
                    "but it doesn't have exploration.");
            }

        }
    }
}
