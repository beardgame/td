using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Components;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Game.Upgrades
{
    sealed class ComponentModifiable : UpgradeEffectBase
    {
        private readonly IComponent component;

        public ComponentModifiable(IComponent component)
        {
            this.component = component;
        }

        public override bool CanApplyTo<T>(ComponentCollection<T> subject) =>
            tryCreateComponentFactory<T>() != null;

        public override void ApplyTo<T>(ComponentCollection<T> subject)
        {
            subject.Add(tryCreateComponentFactory<T>().Create());
        }

        private IComponentFactory<T> tryCreateComponentFactory<T>()
        {
            return ComponentFactories.CreateComponentFactory<T>(component);
        }
    }
}
