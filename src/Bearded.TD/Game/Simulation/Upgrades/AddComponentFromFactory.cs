using Bearded.TD.Content.Serialization.Models;
using IComponent = Bearded.TD.Content.Serialization.Models.IComponent;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class AddComponentFromFactory : AddComponent
{
    private readonly IComponent component;

    public AddComponentFromFactory(IComponent component, UpgradePrerequisites prerequisites, bool isSideEffect)
        : base(prerequisites, isSideEffect)
    {
        this.component = component;
    }

    protected override GameObjects.IComponent CreateComponent() =>
        ComponentFactories.CreateComponentFactory(component).Create();
}
