using Bearded.TD.Game.Components;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Upgrades
{
    interface IUpgradeEffect
    {
        bool CanApplyTo(IAttributeModifiable subject);

        //bool CanApplyTo<T>(ComponentCollection<T> subject);
        //bool CanApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;

        
        void ApplyTo(IAttributeModifiable subject);
        
        //void ApplyTo<T>(ComponentCollection<T> subject);
        //void ApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
    }
}
