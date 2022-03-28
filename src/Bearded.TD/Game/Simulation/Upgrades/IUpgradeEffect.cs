using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Game.Simulation.GameObjects;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades;

interface IUpgradeEffect
{
    bool CanApplyTo(GameObject subject);
    bool CanApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
    bool CanApplyToComponentCollectionForType();

    void ApplyTo(GameObject subject);
    void ApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;

    bool RemoveFrom(GameObject subject);
    bool RemoveFrom<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
}
