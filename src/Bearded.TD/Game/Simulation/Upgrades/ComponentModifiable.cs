using Bearded.TD.Content.Serialization.Models;
using Bearded.TD.Game.Simulation.GameObjects;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class ComponentModifiable : UpgradeEffectBase
{
    private readonly IComponent component;

    public ComponentModifiable(IComponent component, UpgradePrerequisites? prerequisites) : base(prerequisites)
    {
        this.component = component;
    }

    public override void ApplyTo(GameObject subject)
    {
        if (CanApplyTo(subject))
        {
            var factory = ComponentFactories.CreateComponentFactory(component);
            // TODO: this right now breaks, because this is called WHILE also looping over the component list.
            //       Could easily be solved by making the component list a DeletableList. This would also allow
            //       components to delete themselves once they're done.
            //       There are other problems with that (mutability is ew), but perhaps the simplest way out?
            subject.AddComponent(factory.Create());
        }

        base.ApplyTo(subject);
    }
}
