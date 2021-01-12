using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bearded.TD.Game.Simulation.Technologies;

namespace Bearded.TD.Game.Simulation.Rules.Technologies
{
    [GameRule("unlockTechnology")]
    sealed class UnlockTechnology : GameRule<UnlockTechnology.Parameters>
    {
        public UnlockTechnology(Parameters parameters) : base(parameters) {}

        protected override void Execute(GameState gameState, Parameters parameters)
        {
            foreach (var unlock in parameters.Unlocks)
            {
                unlock.Apply(gameState.RootFaction.Technology);
            }
        }

        public readonly struct Parameters
        {
            public IEnumerable<ITechnologyUnlock> Unlocks { get; }

            [JsonConstructor]
            public Parameters(IEnumerable<ITechnologyUnlock> unlocks)
            {
                Unlocks = unlocks;
            }
        }
    }
}
