using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradeEffect
{
    bool CanApplyTo(ComponentGameObject subject);
    bool CanApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
    bool CanApplyToComponentCollectionForType();

    void ApplyTo(ComponentGameObject subject);
    void ApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;

    bool RemoveFrom(ComponentGameObject subject);
    bool RemoveFrom<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
}
