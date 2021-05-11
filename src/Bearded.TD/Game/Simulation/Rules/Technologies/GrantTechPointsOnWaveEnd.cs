using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;
using Newtonsoft.Json;

namespace Bearded.TD.Game.Simulation.Rules.Technologies
{
    [GameRule("grantTechPointsOnWaveEnd")]
    sealed class GrantTechPointsOnWaveEnd : GameRule<GrantTechPointsOnWaveEnd.RuleParameters>
    {
        public GrantTechPointsOnWaveEnd(RuleParameters parameters) : base(parameters) { }

        public override void Initialize(GameRuleContext context)
        {
            context.Events.Subscribe(new Listener(Parameters.Amount));
        }

        private sealed class Listener : IListener<WaveEnded>
        {
            private readonly int amount;

            public Listener(int amount)
            {
                this.amount = amount;
            }

            public void HandleEvent(WaveEnded @event)
            {
                @event.TargetFaction.Technology?.AddTechPoints(amount);
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
