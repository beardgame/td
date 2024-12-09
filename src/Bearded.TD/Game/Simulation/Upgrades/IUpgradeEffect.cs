using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradeEffect
{
    UpgradePrerequisites Prerequisites { get; }
    bool IsSideEffect { get; }

    bool ModifiesComponentCollection(GameObject subject);
    ComponentTransaction CreateComponentChanges(GameObject subject);

    bool ModifiesParameters(IParametersTemplate subject);
    ParameterTransaction CreateParameterChanges(GameObject subject, IParametersTemplate parameters);
}
