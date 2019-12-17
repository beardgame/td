using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IBurnableParameters : IParametersTemplate<IBurnableParameters>
    {
        Energy FuelAmount { get; }
        Energy FlashPointThreshold { get; }
        EnergyConsumptionRate? BurnSpeed { get; }
        double? DamagePerFuel { get; }
        bool StartsOnFire { get; }
    }
}
