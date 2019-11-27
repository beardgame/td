using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game.Components
{
    [ContainsAttributeConverters]
    static class SpaceTimeTypes
    {
        [ConvertsAttribute]
        public static AttributeConverter<Angle> AngleConverter =
            new AttributeConverter<Angle>(
                d => Angle.FromDegrees((float) d), a => a.Degrees);

        [ConvertsAttribute]
        public static AttributeConverter<AngularAcceleration> AngularAccelerationConverter =
            new AttributeConverter<AngularAcceleration>(
                d => AngularAcceleration.FromDegrees((float) d), a => a.AngleValue.Degrees);

        [ConvertsAttribute]
        public static AttributeConverter<Energy> EnergyConverter =
            new AttributeConverter<Energy>(d => new Energy(d), e => e.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<EnergyConsumptionRate> EnergyConsumptionRateConverter =
            new AttributeConverter<EnergyConsumptionRate>(d => new EnergyConsumptionRate(d), e => e.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<FlowRate> FlowRateConverter =
            new AttributeConverter<FlowRate>(d => new FlowRate((float) d), r => r.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Frequency> FrequencyConverter =
            new AttributeConverter<Frequency>(d => new Frequency(d), f => f.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Speed> SpeedConverter =
            new AttributeConverter<Speed>(d => new Speed((float) d), s => s.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<TimeSpan> TimeSpanConverter =
            new AttributeConverter<TimeSpan>(d => new TimeSpan(d), t => t.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Unit> UnitConverter =
            new AttributeConverter<Unit>(d => new Unit((float) d), u => u.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Volume> VolumeConverter =
            new AttributeConverter<Volume>(d => new Volume((float) d), v => v.NumericValue);
    }
}
