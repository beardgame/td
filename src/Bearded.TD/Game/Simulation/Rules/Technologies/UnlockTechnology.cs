using System.Collections.Generic;
using Bearded.TD.Game.Simulation.Technologies;
using Newtonsoft.Json;

namespace Bearded.TD.Game.Simulation.Rules.Technologies
{
    [GameRule("unlockTechnology")]
    sealed class UnlockTechnology : GameRule<UnlockTechnology.RuleParameters>
    {
        public UnlockTechnology(RuleParameters parameters) : base(parameters) {}

        public override void Execute(GameRuleContext context)
        {
            var faction = context.RootFaction;
            foreach (var unlock in Parameters.Unlocks)
            {
                unlock.Apply(faction.Technology!);
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
