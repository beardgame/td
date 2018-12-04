using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Mods.Models
{
    interface IHealthComponentParameter : IParametersTemplate<IHealthComponentParameter>
    {
        [Modifiable(1, AttributeType.Health)]
        int MaxHealth { get; }
    }
}
