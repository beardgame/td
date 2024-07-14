using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.UI.Controls;

sealed class TechTree : IDisposable, IListener<TechnologyUnlocked>, IListener<TierUnlocked>
{
    private readonly FactionTechnology factionTechnology;
    private readonly GlobalGameEvents events;
    public ImmutableDictionary<TechnologyBranch, Branch> Branches { get; }

    private TechTree(
        ImmutableDictionary<TechnologyBranch, Branch> branches,
        FactionTechnology factionTechnology,
        GlobalGameEvents events)
    {
        Branches = branches;
        this.factionTechnology = factionTechnology;
        this.events = events;

        events.Subscribe<TechnologyUnlocked>(this);
        events.Subscribe<TierUnlocked>(this);
    }

    public void HandleEvent(TechnologyUnlocked @event)
    {
        if (factionTechnology != @event.FactionTechnology) return;
        Branches[@event.Technology.Branch]
            .Tiers[@event.Technology.Tier]
            .Technologies.First(t => t.Blueprint == @event.Technology)
            .SetUnlocked();
    }

    public void HandleEvent(TierUnlocked @event)
    {
        if (factionTechnology != @event.FactionTechnology) return;
        Branches[@event.Branch]
            .Tiers[@event.Tier]
            .Technologies
            .ForEach(t => t.CheckUnlockable(factionTechnology));
    }

    public void Dispose()
    {
        events.Unsubscribe<TechnologyUnlocked>(this);
        events.Unsubscribe<TierUnlocked>(this);
    }

    public static TechTree FromBlueprints(
        ReadonlyBlueprintCollection<ITechnologyBlueprint> technologies,
        FactionTechnology factionTechnology,
        GlobalGameEvents events)
    {
        var technologiesByBranchAndTier = technologies.All.ToLookup(t => (t.Branch, t.Tier));

        var branches = Enum.GetValues<TechnologyBranch>();
        var tiers = Enum.GetValues<TechnologyTier>();

        return new TechTree(
            branches.ToImmutableDictionary(
                b => b,
                branch => new Branch(tiers.ToImmutableDictionary(
                    t => t,
                    tier => new Tier(
                        technologiesByBranchAndTier[(branch, tier)]
                            .Select(blueprint => Technology.FromBlueprint(blueprint, factionTechnology))
                            .ToImmutableArray(),
                        tier.GetRequiredForCompletionCount()))
                )),
            factionTechnology,
            events);
    }

    public sealed record Branch(ImmutableDictionary<TechnologyTier, Tier> Tiers);

    public sealed class Tier
    {
        public ImmutableArray<Technology> Technologies { get; }
        public int TechsRequiredForCompletionCount { get; }
        public IReadonlyBinding<int> TechsUnlockedCountBinding { get; }
        public IReadonlyBinding<double> CompletionPercentageBinding { get; }

        public Tier(ImmutableArray<Technology> technologies, int techsRequiredForCompletionCount)
        {
            Technologies = technologies;
            TechsRequiredForCompletionCount = techsRequiredForCompletionCount;
            TechsUnlockedCountBinding = Binding.Aggregate(
                Technologies.Select(t => t.IsUnlockedBinding),
                flags => flags.Count(b => b));
            CompletionPercentageBinding = TechsUnlockedCountBinding.Transform(percentageUnlocked);
        }

        private double percentageUnlocked(int techsUnlocked)
        {
            if (TechsRequiredForCompletionCount == 0) return 0;
            if (techsUnlocked >= TechsRequiredForCompletionCount) return 1;
            return (double)techsUnlocked / TechsRequiredForCompletionCount;
        }
    }

    public sealed class Technology
    {
        public ITechnologyBlueprint Blueprint { get; }
        public Binding<bool> IsUnlockableBinding { get; }
        public Binding<bool> IsUnlockedBinding { get; }

        private Technology(ITechnologyBlueprint blueprint, bool isUnlockable, bool isUnlocked)
        {
            Blueprint = blueprint;
            IsUnlockableBinding = Binding.Create(isUnlockable);
            IsUnlockedBinding = Binding.Create(isUnlocked);
        }

        public void CheckUnlockable(FactionTechnology technology)
        {
            IsUnlockableBinding.SetFromSource(technology.CanUnlockTechnology(Blueprint));
        }

        public void SetUnlocked()
        {
            IsUnlockableBinding.SetFromSource(false);
            IsUnlockedBinding.SetFromSource(true);
        }

        public static Technology FromBlueprint(ITechnologyBlueprint blueprint, FactionTechnology factionTechnology)
        {
            return new Technology(
                blueprint,
                factionTechnology.CanUnlockTechnology(blueprint),
                !factionTechnology.IsTechnologyLocked(blueprint));
        }
    }
}
