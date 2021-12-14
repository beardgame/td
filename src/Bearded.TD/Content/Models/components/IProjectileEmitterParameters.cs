using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.Geometry;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

interface IProjectileEmitterParameters : IParametersTemplate<IProjectileEmitterParameters>
{
    IComponentOwnerBlueprint Projectile { get; }

    [Modifiable(6.6, Type = AttributeType.FireRate)]
    Frequency FireRate { get; }

    [Modifiable(10.0)]
    Speed MuzzleSpeed { get; }

    [Modifiable(0.0, Type = AttributeType.SpreadAngle)]
    Angle Spread { get; }

    Unit MuzzleOffset { get; }
}