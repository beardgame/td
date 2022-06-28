using System.Linq;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;
using Bearded.TD.Utilities.Collections;

namespace Bearded.TD.Game.Simulation.Upgrades;

abstract class UpgradeEffectBase : IUpgradeEffect
{
    public bool CanApplyTo(GameObject subject)
        => subject.GetComponents<IComponent>().Any(c => c.CanApplyUpgradeEffect(this));

    public virtual bool CanApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T> => false;

    public virtual void ApplyTo(GameObject subject)
        => subject.GetComponents<IComponent>().ForEach(c => c.ApplyUpgradeEffect(this));

    public virtual void ApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T> {}

    public virtual bool RemoveFrom(GameObject subject)
        => subject.GetComponents<IComponent>().Aggregate(false, (b, c) => c.RemoveUpgradeEffect(this) || b);

    public virtual bool RemoveFrom<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T> => false;
}
