using Bearded.TD.Game.GameState.Components;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.GameState.Upgrades
{
    interface IUpgradeEffect
    {
        bool CanApplyTo<T>(ComponentCollection<T> subject);
        bool CanApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
        bool CanApplyToComponentCollectionForType<T>();

        void ApplyTo<T>(ComponentCollection<T> subject);
        void ApplyTo<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;

        bool RemoveFrom<T>(ComponentCollection<T> subject);
        bool RemoveFrom<T>(IParametersTemplate<T> subject) where T : IParametersTemplate<T>;
    }
}
