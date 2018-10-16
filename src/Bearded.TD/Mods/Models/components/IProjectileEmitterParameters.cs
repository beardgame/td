﻿using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Mods.Models
{
    interface IProjectileEmitterParameters : IParametersTemplate<IProjectileEmitterParameters>
    {
        [Modifiable]
        ProjectileBlueprint Projectile { get; }

        [Modifiable(.15, Type = AttributeType.Cooldown)]
        TimeSpan ShootInterval { get; }

        [Modifiable(10.0)]
        Speed MuzzleVelocity { get; }
    }
}
