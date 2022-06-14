using System;
using System.Collections.Immutable;
using System.Linq;
using Bearded.TD.Game;
using Bearded.TD.Game.Simulation.Events;
using Bearded.TD.Game.Simulation.Technologies;
using Bearded.TD.Shared.Events;
using Bearded.TD.Utilities;

namespace Bearded.TD.UI.Controls;

sealed class TechTree : IDisposable, IListener<TechnologyUnlocked>
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

        events.Subscribe(this);
    }

    public void HandleEvent(TechnologyUnlocked @event)
    {
        if (factionTechnology != @event.FactionTechnology) return;
        Branches[@event.Technology.Branch]
            .Tiers[@event.Technology.Tier]
            .Technologies.First(t => t.Blueprint == @event.Technology)
            .SetUnlocked();
    }

    public void Dispose()
    {
        events.Unsubscribe(this);
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

    public sealed class Branch
    {
        public ImmutableDictionary<TechnologyTier, Tier> Tiers { get; }

        public Branch(ImmutableDictionary<TechnologyTier, Tier> tiers)
        {
            Tiers = tiers;
        }
    }

    public sealed class Tier
    {
        public ImmutableArray<Technology> Technologies { get; }
        public int TechsRequiredForCompletionCount { get; }
        public int TechsUnlockedCount { get; }
        public Binding<int> TechsUnlockedCountBinding { get; }

        public Tier(ImmutableArray<Technology> technologies, int techsRequiredForCompletionCount)
        {
            Technologies = technologies;
            TechsRequiredForCompletionCount = techsRequiredForCompletionCount;
            TechsUnlockedCount = technologies.Count(t => t.IsUnlocked);
            TechsUnlockedCountBinding = Binding.Aggregate(
                Technologies.Select(t => t.IsUnlockedBinding),
                flags => flags.Count(b => b));
        }
    }

    public sealed class Technology
    {
        public ITechnologyBlueprint Blueprint { get; }
        public bool IsUnlocked { get; private set; }
        public Binding<bool> IsUnlockedBinding { get; }

        private Technology(ITechnologyBlueprint blueprint, bool isUnlocked)
        {
            Blueprint = blueprint;
            IsUnlocked = isUnlocked;
            IsUnlockedBinding = Binding.Create(IsUnlocked);
        }

        public void SetUnlocked()
        {
            IsUnlocked = true;
            IsUnlockedBinding.SetFromSource(true);
        }

        public static Technology FromBlueprint(ITechnologyBlueprint blueprint, FactionTechnology factionTechnology)
        {
            return new Technology(blueprint, !factionTechnology.IsTechnologyLocked(blueprint));
        }
    }
}
