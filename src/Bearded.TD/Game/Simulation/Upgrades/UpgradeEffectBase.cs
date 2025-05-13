using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Game.Simulation.GameObjects.Parameters;

namespace Bearded.TD.Game.Simulation.Upgrades;

abstract class UpgradeEffectBase : IUpgradeEffect
{
    public UpgradePrerequisites Prerequisites { get; }
    public bool IsSideEffect { get; }

    protected UpgradeEffectBase(UpgradePrerequisites prerequisites, bool isSideEffect)
    {
        Prerequisites = prerequisites;
        IsSideEffect = isSideEffect;
    }

    public virtual bool ModifiesComponentCollection(GameObject subject) => false;

    public virtual ComponentTransaction CreateComponentChanges(GameObject subject) =>
        ComponentTransaction.Empty(subject);

    public virtual bool ModifiesParameters(IParametersTemplate subject) => false;

    public virtual ParameterTransaction CreateParameterChanges(IParametersTemplate parameters) =>
        ParameterTransaction.Empty(parameters);
}
