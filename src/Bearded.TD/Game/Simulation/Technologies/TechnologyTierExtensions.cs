namespace Bearded.TD.Game.Simulation.Technologies;

static class TechnologyTierExtensions
{
    public static int GetRequiredForCompletionCount(this TechnologyTier tier) =>
        Constants.Game.Technology.TierCompletionThresholds[(int) tier];
}
