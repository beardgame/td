using System;
using Bearded.TD.Game.Simulation.GameObjects;

namespace Bearded.TD.Game.Simulation.Upgrades;

sealed class AddComponentFromDelegate(
        Func<IComponent> componentFactory, UpgradePrerequisites prerequisites, bool isSideEffect)
    : AddComponent(prerequisites, isSideEffect)
{
    protected override IComponent CreateComponent()
    {
        return componentFactory();
    }
}
