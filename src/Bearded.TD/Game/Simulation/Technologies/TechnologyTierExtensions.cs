using System;

namespace Bearded.TD.Game.Simulation.Technologies;

static class TechnologyTierExtensions
{
    public static int GetRequiredForCompletionCount(this TechnologyTier tier) =>
        Constants.Game.Technology.TierCompletionThresholds[(int) tier];

    public static TechnologyTier? NextTier(this TechnologyTier tier)
    {
        return tier switch
        {
            TechnologyTier.Free => null,
            TechnologyTier.Low => TechnologyTier.Medium,
            TechnologyTier.Medium => TechnologyTier.High,
            TechnologyTier.High => null,
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, null)
        };
    }
}
