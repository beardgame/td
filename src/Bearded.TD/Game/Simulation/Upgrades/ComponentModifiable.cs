using Bearded.TD.Content.Serialization.Models;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class ComponentModifiable : UpgradeEffectBase
{
    private readonly IComponent component;

    public ComponentModifiable(IComponent component, UpgradePrerequisites prerequisites, bool isSideEffect)
        : base(prerequisites, isSideEffect)
    {
        this.component = component;
    }

    public override bool ContributesComponent => true;

    public override GameObjects.IComponent CreateComponent() =>
        ComponentFactories.CreateComponentFactory(component).Create();
}
