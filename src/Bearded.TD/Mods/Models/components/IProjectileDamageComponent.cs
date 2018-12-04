using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IProjectileDamageComponent : IParametersTemplate<IProjectileDamageComponent>
    {
        [Modifiable(Type = AttributeType.Damage)]
        int Damage { get; }
    }
}
