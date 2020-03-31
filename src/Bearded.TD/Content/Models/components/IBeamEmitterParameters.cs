using amulware.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IBeamEmitterParameters : IParametersTemplate<IBeamEmitterParameters>
    {
        [Modifiable(10, Type = AttributeType.Damage)]
        int DamagePerSecond { get; }

        [Modifiable(Type = AttributeType.Range)]
        Unit Range { get; }

        [Modifiable(0.0f, Type = AttributeType.PiercingFactor)]
        float PiercingFactor { get; }

        Color Color { get; }
    }
}
