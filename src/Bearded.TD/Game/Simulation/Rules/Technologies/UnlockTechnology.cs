using System.Collections.Generic;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Technologies;

namespace Bearded.TD.Game.Simulation.Rules.Technologies
{
    [GameRule("unlockTechnology")]
    sealed class UnlockTechnology : GameRule<UnlockTechnology.RuleParameters>
    {
        public UnlockTechnology(RuleParameters parameters) : base(parameters) {}

        public override void Execute(GameRuleContext context)
        {
            var faction = context.Factions.Find(Parameters.Faction);
            foreach (var unlock in Parameters.Unlocks)
            {
                unlock.Apply(faction.Technology!);
            }
        }

        public sealed record RuleParameters(ExternalId<Faction> Faction, IEnumerable<ITechnologyUnlock> Unlocks);
    }
}
