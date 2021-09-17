using Bearded.TD.Game.Simulation.GameLoop;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.Utilities.IO;
using Newtonsoft.Json;

namespace Bearded.TD.Game.Simulation.Rules.Technologies
{
    [GameRule("grantTechPointsOnWaveEnd")]
    sealed class GrantTechPointsOnWaveEnd : GameRule<GrantTechPointsOnWaveEnd.RuleParameters>
    {
        public GrantTechPointsOnWaveEnd(RuleParameters parameters) : base(parameters) { }

        public override void Execute(GameRuleContext context)
        {
            context.Events.Subscribe(new Listener(context.Logger, Parameters.Amount));
        }

        private sealed class Listener : IListener<WaveEnded>
        {
            private readonly Logger logger;
            private readonly int amount;

            public Listener(Logger logger, int amount)
            {
                this.logger = logger;
                this.amount = amount;
            }

            public void HandleEvent(WaveEnded @event)
            {
                if (@event.TargetFaction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology))
                {
                    technology.AddTechPoints(amount);
                }
                else
                {
                    logger.Debug?.Log(
                        $"Tried awarding tech points after wave end to {@event.TargetFaction.ExternalId}, " +
                        "but it doesn't have technology.");
                }
            }
        }

        public readonly struct RuleParameters
        {
            public int Amount { get; }

            [JsonConstructor]
            public RuleParameters(int? amount = null)
            {
                Amount = amount ?? 1;
            }
        }
    }
}
