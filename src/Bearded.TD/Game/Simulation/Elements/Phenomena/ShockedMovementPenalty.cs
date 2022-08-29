using System;
using Bearded.TD.Game.Simulation.Damage;

namespace Bearded.TD.Game.Simulation.Elements.Phenomena;

static class ShockedMovementPenalty
{
    private const double maxMovementPenalty = 0.5;
    private static readonly UntypedDamagePerSecond halfMovementDamageThreshold = new(20.HitPoints());

    public static double ToMovementPenalty(UntypedDamagePerSecond dps)
    {
        const double scale = maxMovementPenalty * 2 / Math.PI;

        var r = (double) dps.Amount.NumericValue / halfMovementDamageThreshold.Amount.NumericValue;
        return maxMovementPenalty * Math.Atan(r);
    }
}
