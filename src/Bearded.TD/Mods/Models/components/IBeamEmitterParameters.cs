﻿using amulware.Graphics;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface IBeamEmitterParameters : IParametersTemplate<IBeamEmitterParameters>
    {
        [Modifiable(10, Type = AttributeType.Damage)]
        int DamagePerSecond { get; }

        [Modifiable(Type = AttributeType.Range)]
        Unit Range { get; }

        [Modifiable(0xFFFFA500 /* orange */)]
        Color Color { get; }
    }
}
