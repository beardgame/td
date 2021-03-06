using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IDamageBuildingsInRangeParameters : IParametersTemplate<IDamageBuildingsInRangeParameters>
    {
        [Modifiable(Type = AttributeType.Damage)]
        HitPoints Damage { get; }

        [Modifiable(Type = AttributeType.FireRate)]
        Frequency AttackRate { get; }
    }
}
