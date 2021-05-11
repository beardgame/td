using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Technologies;
using Newtonsoft.Json;

namespace Bearded.TD.Game.Simulation.Rules.Technologies
{
    [GameRule("unlockTechnology")]
    sealed class UnlockTechnology : GameRule<UnlockTechnology.RuleParameters>
    {
        public UnlockTechnology(RuleParameters parameters) : base(parameters) {}

        protected override void Execute(GameState gameState, RuleParameters parameters)
        {
            foreach (var unlock in parameters.Unlocks)
            {
                unlock.Apply(gameState.RootFaction.Technology);
            }
        }

        public readonly struct RuleParameters
        {
            public IEnumerable<ITechnologyUnlock> Unlocks { get; }

            [JsonConstructor]
            public RuleParameters(IEnumerable<ITechnologyUnlock> unlocks)
            {
                Unlocks = unlocks;
            }
        }
    }
}
