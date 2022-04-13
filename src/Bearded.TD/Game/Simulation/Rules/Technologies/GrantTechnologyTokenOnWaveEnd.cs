using System.Collections.Immutable;
using System.Linq;
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
        var technologies = context.Blueprints.Technologies.All.ToImmutableArray();
        context.Events.Subscribe(new Listener(context.Logger, technologies));
    }

    private sealed class Listener : IListener<WaveEnded>
    {
        private readonly Logger logger;
        private readonly ImmutableArray<ITechnologyBlueprint> technologies;

        public Listener(Logger logger, ImmutableArray<ITechnologyBlueprint> technologies)
        {
            this.logger = logger;
            this.technologies = technologies;
        }

        public void HandleEvent(WaveEnded @event)
        {
            if (!@event.TargetFaction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology))
            {
                logger.Debug?.Log(
                    $"Tried awarding a technology token after wave end to {@event.TargetFaction.ExternalId}, " +
                    "but it doesn't have technology.");
                return;
            }

            var hasTokenAlready = technology.HasTechnologyToken;
            var hasAvailableTech = technologies.Any(technology.CanUnlockTechnology);

            if (hasAvailableTech && !hasTokenAlready)
            {
                technology.AwardTechnologyToken();
            }
        }
    }

    public readonly struct RuleParameters {}
}
