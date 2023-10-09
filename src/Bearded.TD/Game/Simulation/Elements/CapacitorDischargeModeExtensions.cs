using System;

namespace Bearded.TD.Game.Simulation.Elements;

static class CapacitorDischargeModeExtensions
{
    public static float MinimumChargePercentage(this CapacitorDischargeMode mode) => mode switch
    {
        CapacitorDischargeMode.FullCharge => 1f,
        CapacitorDischargeMode.MinimumCharge => 0.1667f,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };

    public static string Name(this CapacitorDischargeMode mode) => mode switch
    {
        CapacitorDischargeMode.FullCharge => "Full charge",
        CapacitorDischargeMode.MinimumCharge => "Minimum charge",
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
    };
}
