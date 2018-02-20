using Bearded.TD.Utilities;
using Bearded.Utilities.SpaceTime;
using Newtonsoft.Json;

namespace Bearded.TD.Mods.Models
{
    sealed class StatusEmitterParameters
    {
        public Unit Range { get; } = 5.U();

        public TimeSpan RecalculateTilesInRangeInterval { get; } = 1.S();
        public TimeSpan RecalculateUnitsInRangeInterval { get; } = .1.S();

        public StatusEmitterParameters() { }

        [JsonConstructor]
        public StatusEmitterParameters(Unit? range)
        {
            Range = range ?? Range;
        }
    }
}
