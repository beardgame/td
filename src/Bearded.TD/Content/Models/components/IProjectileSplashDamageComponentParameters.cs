using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models;

interface IProjectileSplashDamageComponentParameters : IParametersTemplate<IProjectileSplashDamageComponentParameters>
{
    [Modifiable(Type = AttributeType.Damage)]
    HitPoints Damage { get; }

    [Modifiable(Type = AttributeType.SplashRange)]
    Unit Range { get; }
}