using Bearded.TD.Game.Components;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Upgrades
{
    sealed class ComponentModifiable<TComponentOwner, TComponentParameters> : UpgradeEffectBase
        where TComponentParameters : IParametersTemplate<TComponentParameters>
    {
        private readonly ComponentFactory<TComponentOwner, TComponentParameters> componentFactory;

        public ComponentModifiable(ComponentFactory<TComponentOwner, TComponentParameters> componentFactory)
        {
            this.componentFactory = componentFactory;
        }

        public override bool CanApplyTo<T>(ComponentCollection<T> subject) => typeof(T) == typeof(TComponentOwner);

        public override void ApplyTo<T>(ComponentCollection<T> subject)
        {
            subject.Add((IComponent<T>) componentFactory.Create());
        }
    }
}
