using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface IStatusEmitterParameters : ITechEffectModifiable
    {
        [Modifiable(5f)]
        Unit Range { get; }

        [Modifiable(1.0)]
        TimeSpan RecalculateTilesInRangeInterval { get; }

        [Modifiable(.1)]
        TimeSpan RecalculateUnitsInRangeInterval { get; }
    }
}
