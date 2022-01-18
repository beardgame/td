using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.IO;

namespace Bearded.TD.Game.Simulation.Rules.Technologies;

[GameRule("grantTechnologyTokenOnWaveEnd")]
sealed class GrantTechnologyTokenOnWaveEnd : GameRule<GrantTechnologyTokenOnWaveEnd.RuleParameters>
{
    public GrantTechnologyTokenOnWaveEnd(RuleParameters parameters) : base(parameters) { }

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
            if (@event.TargetFaction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology))
            {
                technology.AwardTechnologyToken();
            }
            else
            {
                logger.Debug?.Log(
                    $"Tried awarding a technology token after wave end to {@event.TargetFaction.ExternalId}, " +
                    "but it doesn't have technology.");
            }
        }
    }

    public readonly struct RuleParameters {}
}
