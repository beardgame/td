using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    interface IStatusEffectEmitterParameters : IParametersTemplate<IStatusEffectEmitterParameters>
    {
        AttributeType AttributeAffected { get; }
        
        [Modifiable(Type = AttributeType.EffectStrength)]
        double ModificationValue { get; }
        
        [Modifiable(Type = AttributeType.Range)]
        Unit Range { get; }
    }
}
