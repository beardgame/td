using System;
using System.Collections.Generic;
using Bearded.TD.Content.Mods;
using Bearded.TD.Game.Simulation.Factions;
using Bearded.TD.Game.Simulation.Rules;
using Bearded.TD.Shared.Events;
using JetBrains.Annotations;

namespace Bearded.TD.Game.Simulation.Technologies.rules;

[GameRule("unlockTiersAtThresholds")]
sealed class UnlockTiersAtThresholds : GameRule<UnlockTiersAtThresholds.RuleParameters>
{
    public UnlockTiersAtThresholds(RuleParameters parameters) : base(parameters) { }

    public override void Execute(GameRuleContext context)
    {
        var faction = context.Factions.Find(Parameters.Faction);
        if (!faction.TryGetBehavior<FactionTechnology>(out var technology))
        {
            context.Logger.Warning?.Log(
                $"Attempted to implement tier unlocking logic for faction without technology: {Parameters.Faction}");
            return;
        }

        var tierUnlocker = new TierUnlocker(technology);
        context.Events.Subscribe(tierUnlocker);

        foreach (var branch in Enum.GetValues<TechnologyBranch>())
        {
            technology.UnlockTier(branch, TechnologyTier.Low);
        }
    }

    private sealed class TierUnlocker(FactionTechnology technology) : IListener<TechnologyUnlocked>
    {
        private readonly Dictionary<TechnologyBranchTier, int> unlockedCountsByTier = new();

        public void HandleEvent(TechnologyUnlocked @event)
        {
            if (@event.FactionTechnology == technology)
            {
                onTechnologyUnlocked(@event.Technology);
            }
        }

        private void onTechnologyUnlocked(ITechnologyBlueprint blueprint)
        {
            if (blueprint.Tier == TechnologyTier.Free) return;
            var branchTier = new TechnologyBranchTier(blueprint.Branch, blueprint.Tier);
            var existingCount = unlockedCountsByTier.GetValueOrDefault(branchTier);
            var newCount = existingCount + 1;
            unlockedCountsByTier[branchTier] = newCount;

            var nextTier = branchTier.NextTier;
            if (nextTier != null &&
                !technology.IsTierUnlocked(nextTier.Value) &&
                newCount >= blueprint.Tier.GetRequiredForCompletionCount())
            {
                technology.UnlockTier(nextTier.Value);
            }
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public sealed record RuleParameters(ExternalId<Faction> Faction);
}
