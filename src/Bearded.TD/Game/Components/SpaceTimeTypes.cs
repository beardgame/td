using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    [ContainsAttributeConverters]
    static class SpaceTimeTypes
    {
        [ConvertsAttribute]
        public static AttributeConverter<Frequency> FrequencyConverter =
            new AttributeConverter<Frequency>(d => new Frequency(d), f => f.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Speed> SpeedConverter =
            new AttributeConverter<Speed>(d => new Speed((float) d), s => s.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Unit> UnitConverter =
            new AttributeConverter<Unit>(d => new Unit((float) d), u => u.NumericValue);
    }
}
