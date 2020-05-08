using System.Collections.Generic;
using Bearded.TD.Game.Technologies;
using Newtonsoft.Json;

namespace Bearded.TD.Game.Rules.Technologies
{
    [GameRule("unlockTechnologies")]
    sealed class UnlockTechnologiesRule : GameRule<UnlockTechnologiesRule.Parameters>
    {
        public UnlockTechnologiesRule(Parameters parameters) : base(parameters) {}

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
