using Bearded.TD.Game.Simulation.Components;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Upgrades
{
    interface IUpgradeEffect
    {
        bool CanApplyTo<T>(T subject) where T : IComponentOwner;
        bool CanApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
        bool CanApplyToComponentCollectionForType<T>();

        void ApplyTo<T>(T subject) where T : IComponentOwner<T>;
        void ApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;

        bool RemoveFrom(IComponentOwner subject);
        bool RemoveFrom<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
    }
}
