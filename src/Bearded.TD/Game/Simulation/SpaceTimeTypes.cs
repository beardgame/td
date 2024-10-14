using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Game.Simulation.Elements;
using Bearded.TD.Game.Simulation.Resources;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;
using TimeSpan = Bearded.Utilities.SpaceTime.TimeSpan;

namespace Bearded.TD.Game.Simulation;

static class SpaceTimeTypes
{
    [ConvertsAttribute]
    public static AttributeConverter<Acceleration> AccelerationConverter =
        new(d => new Acceleration((float) d), a => a.NumericValue);

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
    public static AttributeConverter<UntypedDamage> UntypedDamageConverter =
        new(d => new UntypedDamage(new HitPoints((int) d)), d => d.Amount.NumericValue);

    [ConvertsAttribute]
    public static AttributeConverter<UntypedDamagePerSecond> UntypedDpsConverter =
        new(d => new UntypedDamagePerSecond(new HitPoints((int) d)), dps => dps.Amount.NumericValue);

    [ConvertsAttribute]
    public static AttributeConverter<ElectricCharge> ElectricChargeConverter =
        new(d => new ElectricCharge((float) d), e => e.NumericValue);

    [ConvertsAttribute]
    public static AttributeConverter<ElectricChargeRate> ElectricChargeRateConverter =
        new(d => new ElectricChargeRate((float) d), e => e.NumericValue);

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
    public static AttributeConverter<Resource<Scrap>> ScrapConverter =
        new(d => new Resource<Scrap>(d), r => r.Value);

    [ConvertsAttribute]
    public static AttributeConverter<ResourcePerSecond<Scrap>> ScrapPerSecondConverter =
        new(d => new ResourcePerSecond<Scrap>(d), r => r.Value);

    [ConvertsAttribute]
    public static AttributeConverter<Resource<CoreEnergy>> CoreEnergyConverter =
        new(d => new Resource<CoreEnergy>(d), r => r.Value);

    [ConvertsAttribute]
    public static AttributeConverter<ResourcePerSecond<CoreEnergy>> CoreEnergyPerSecondConverter =
        new(d => new ResourcePerSecond<CoreEnergy>(d), r => r.Value);

    [ConvertsAttribute]
    public static AttributeConverter<ExchangeRate<CoreEnergy, Scrap>> CoreEnergyToScrapRateConverter =
        new(d => new ExchangeRate<CoreEnergy, Scrap>(d), r => r.Value);

    [ConvertsAttribute]
    public static AttributeConverter<ExchangeRate<Scrap, CoreEnergy>> ScrapToCoreEnergyRateConverter =
        new(d => new ExchangeRate<Scrap, CoreEnergy>(d), r => r.Value);

    [ConvertsAttribute]
    public static AttributeConverter<Speed> SpeedConverter =
        new(d => new Speed((float) d), s => s.NumericValue);

    [ConvertsAttribute]
    public static AttributeConverter<TemperatureDifference> TemperatureDifferenceConverter =
        new(d => new TemperatureDifference((float) d), td => td.NumericValue);

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
