using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradeEffect
{
    UpgradePrerequisites Prerequisites { get; }
    bool IsSideEffect { get; }

    bool ModifiesComponentCollection { get; }
    ComponentTransaction CreateComponentChanges(GameObject gameObject);

    bool CanApplyTo(IParametersTemplate subject);
    void ApplyTo(IParametersTemplate subject);
    bool RemoveFrom(IParametersTemplate subject);
}
