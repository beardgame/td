using Bearded.TD.Shared.TechEffects;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Content.Models
{
    public interface IProjectileSplashDamageComponentParameters : IParametersTemplate<IProjectileSplashDamageComponentParameters>
    {
        [Modifiable(Type = AttributeType.Damage)]
        int Damage { get; }
        
        [Modifiable(Type = AttributeType.SplashRange)]
        Unit Range { get; }
    }
}
