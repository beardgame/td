using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.GameState.Components;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Game.GameState.Upgrades
{
    sealed class ComponentModifiable : UpgradeEffectBase
    {
        private readonly IComponent component;

        public ComponentModifiable(IComponent component)
        {
            this.component = component;
        }

        public override bool CanApplyToComponentCollectionForType<T>() => tryCreateComponentFactory<T>() != null;

        public override void ApplyTo<T>(ComponentCollection<T> subject)
        {
            var factory = tryCreateComponentFactory<T>();
            if (factory != null)
            {
                subject.Add(factory.Create());
            }
            base.ApplyTo(subject);
        }

        private IComponentFactory<T> tryCreateComponentFactory<T>() =>
            ComponentFactories.CreateComponentFactory<T>(component);
    }
}
