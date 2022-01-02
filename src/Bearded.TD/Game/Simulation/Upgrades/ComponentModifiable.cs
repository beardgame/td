using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.Components;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class ComponentModifiable : UpgradeEffectBase
{
    private readonly IComponent component;

    public ComponentModifiable(IComponent component)
    {
        this.component = component;
    }

    public override bool CanApplyToComponentCollectionForType<T>() => tryCreateComponentFactory<T>() != null;

    public override void ApplyTo<T>(T subject)
    {
        var factory = tryCreateComponentFactory<T>();
        if (factory != null)
        {
            // TODO: this right now breaks, because this is called WHILE also looping over the component list.
            //       Could easily be solved by making the component list a DeletableList. This would also allow
            //       components to delete themselves once they're done.
            //       There are other problems with that (mutability is ew), but perhaps the simplest way out?
            subject.AddComponent(factory.Create());
        }
        base.ApplyTo(subject);
    }

    private IComponentFactory<T>? tryCreateComponentFactory<T>() =>
        ComponentFactories.CreateComponentFactory<T>(component);
}