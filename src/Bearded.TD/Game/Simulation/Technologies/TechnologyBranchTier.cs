namespace Bearded.TD.Game.Simulation.Technologies;

readonly record struct TechnologyBranchTier(TechnologyBranch Branch, TechnologyTier Tier)
{
    public TechnologyBranchTier? NextTier =>
        Tier.NextTier() is { } nextTier ? this with { Tier = nextTier } : null;
}
