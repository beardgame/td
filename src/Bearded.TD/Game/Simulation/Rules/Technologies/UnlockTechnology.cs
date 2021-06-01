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
            if (!faction.TryGetBehaviorIncludingAncestors<FactionTechnology>(out var technology))
            {
                context.Logger.Warning?.Log(
                    $"Attempted to unlock technologies for {Parameters.Faction}, but it does not support technology.");
                return;
            }
            foreach (var unlock in Parameters.Unlocks)
            {
                unlock.Apply(technology);
            }
        }

        public sealed record RuleParameters(ExternalId<Faction> Faction, IEnumerable<ITechnologyUnlock> Unlocks);
    }
}
