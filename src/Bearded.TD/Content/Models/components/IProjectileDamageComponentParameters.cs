using Bearded.TD.Game.Simulation.Damage;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models
{
    interface IProjectileDamageComponentParameters : IParametersTemplate<IProjectileDamageComponentParameters>
    {
        [Modifiable(Type = AttributeType.Damage)]
        HitPoints Damage { get; }

        DamageType? Type { get; }
    }
}
