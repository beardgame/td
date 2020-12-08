using Bearded.TD.Game.GameState.Damage;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Content.Models
{
    interface IProjectileDamageComponentParameters : IParametersTemplate<IProjectileDamageComponentParameters>
    {
        [Modifiable(Type = AttributeType.Damage)]
        int Damage { get; }

        DamageType? Type { get; }
    }
}
