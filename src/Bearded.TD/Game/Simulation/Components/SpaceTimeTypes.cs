using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation.Components
{
    static class SpaceTimeTypes
    {
        [ConvertsAttribute]
        public static AttributeConverter<Angle> AngleConverter =
            new(d => Angle.FromDegrees((float) d), a => a.Degrees);

        [ConvertsAttribute]
        public static AttributeConverter<AngularAcceleration> AngularAccelerationConverter =
            new(d => AngularAcceleration.FromDegrees((float) d), a => a.AngleValue.Degrees);

        [ConvertsAttribute]
        public static AttributeConverter<AngularVelocity> AngularVelocityConverter =
            new(d => AngularVelocity.FromDegrees((float) d), a => a.AngleValue.Degrees);

        [ConvertsAttribute]
        public static AttributeConverter<Energy> EnergyConverter =
            new(d => new Energy(d), e => e.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<EnergyConsumptionRate> EnergyConsumptionRateConverter =
            new(d => new EnergyConsumptionRate(d), e => e.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<FlowRate> FlowRateConverter =
            new(d => new FlowRate((float) d), r => r.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Frequency> FrequencyConverter =
            new(d => new Frequency(d), f => f.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<HitPoints> HitPointsConverter =
            new(d => new HitPoints((int) d), h => h.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<ResourceAmount> ResourceAmountConverter =
            new(d => new ResourceAmount((int) d), r => r.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Speed> SpeedConverter =
            new(d => new Speed((float) d), s => s.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<TimeSpan> TimeSpanConverter =
            new(d => new TimeSpan(d), t => t.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Unit> UnitConverter =
            new(d => new Unit((float) d), u => u.NumericValue);

        [ConvertsAttribute]
        public static AttributeConverter<Volume> VolumeConverter =
            new(d => new Volume((float) d), v => v.NumericValue);
    }
}
