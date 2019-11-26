using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface ILinearAccelerationAimParameters : IParametersTemplate<ILinearAccelerationAimParameters>
    {
        [Modifiable(180, Type = AttributeType.TurnSpeed)]
        AngularAcceleration Acceleration { get; }

        double DragInverse { get; }
    }
}
