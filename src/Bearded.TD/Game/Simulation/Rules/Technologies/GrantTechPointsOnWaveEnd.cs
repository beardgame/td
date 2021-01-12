using System.Text.Json.Serialization;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.GameLoop;

namespace Bearded.TD.Game.Simulation.Rules.Technologies
{
    [GameRule("grantTechPointsOnWaveEnd")]
    sealed class GrantTechPointsOnWaveEnd : GameRule<GrantTechPointsOnWaveEnd.Parameters>
    {
        public GrantTechPointsOnWaveEnd(Parameters parameters) : base(parameters) { }

        protected override void RegisterEvents(
            GlobalGameEvents events, GameSettings gameSettings, Parameters parameters)
        {
            base.RegisterEvents(events, gameSettings, parameters);
            events.Subscribe(new Listener(parameters.Amount));
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

        public readonly struct Parameters
        {
            public int Amount { get; }

            [JsonConstructor]
            public Parameters(int? amount = null)
            {
                Amount = amount ?? 1;
            }
        }
    }
}
