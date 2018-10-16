using amulware.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface IBeamEmitterParameters : IParametersTemplate<IBeamEmitterParameters>
    {
        [Modifiable(10, Type = AttributeType.DamagePerUnit)]
        int DamagePerSecond { get; }

        [Modifiable(5f)]
        Unit Range { get; }

        [Modifiable(0xFFFFA500 /* orange */)]
        Color Color { get; }
    }
}
