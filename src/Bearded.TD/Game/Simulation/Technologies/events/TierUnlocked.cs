using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Technologies;

readonly record struct TierUnlocked(FactionTechnology FactionTechnology, TechnologyBranchTier BranchTier)
    : IGlobalEvent
{
    public TechnologyBranch Branch => BranchTier.Branch;
    public TechnologyTier Tier => BranchTier.Tier;
}
