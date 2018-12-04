using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IProjectileDamageComponentParameters : IParametersTemplate<IProjectileDamageComponentParameters>
    {
        [Modifiable(Type = AttributeType.Damage)]
        int Damage { get; }
    }
}
