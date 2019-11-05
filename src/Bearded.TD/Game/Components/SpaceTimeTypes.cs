using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    [ContainsAttributeConverters]
    static class SpaceTimeTypes
    {
        [ConvertsAttribute]
        public static AttributeConverter<AngularAcceleration> AngularAccelerationConverter =
            new AttributeConverter<AngularAcceleration>(
                d => AngularAcceleration.FromDegrees((float) d), a => a.AngleValue.Degrees);

        [ConvertsAttribute]
        public static AttributeConverter<Frequency> FrequencyConverter =
            new AttributeConverter<Frequency>(d => new Frequency(d), f => f.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Speed> SpeedConverter =
            new AttributeConverter<Speed>(d => new Speed((float) d), s => s.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Unit> UnitConverter =
            new AttributeConverter<Unit>(d => new Unit((float) d), u => u.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Volume> VolumeConverter =
            new AttributeConverter<Volume>(d => new Volume((float) d), v => v.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<FlowRate> FlowRateConverter =
            new AttributeConverter<FlowRate>(d => new FlowRate((float) d), r => r.NumericValue);
    }
}
