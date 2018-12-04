using System.Linq;
using Bearded.TD.Game.Components;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Upgrades
{
    abstract class UpgradeEffectBase : IUpgradeEffect
    {
        public virtual bool CanApplyTo<T>(ComponentCollection<T> subject)
            => subject.Components.Any(c => c.CanApplyUpgradeEffect(this));
        
        public virtual bool CanApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T> => false;

        public virtual void ApplyTo<T>(ComponentCollection<T> subject)
            => subject.Components.ForEach(c => c.ApplyUpgradeEffect(this));

        public virtual void ApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>
        {
            throw new System.InvalidOperationException("Cannot apply upgrade effect to parameters template.");
        }
    }
}
