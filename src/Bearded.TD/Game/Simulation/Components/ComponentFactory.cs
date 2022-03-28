using System;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Components;

sealed class ComponentFactory<TComponentParameters> : IComponentFactory
    where TComponentParameters : IParametersTemplate<TComponentParameters>
{
    private readonly TComponentParameters parameters;
    private readonly Func<TComponentParameters, IComponent> factory;

    public ComponentFactory(TComponentParameters parameters, Func<TComponentParameters, IComponent> factory)
    {
        this.parameters = parameters;
        this.factory = factory;
    }

    public IComponent Create() => factory(parameters);

    public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => effect.CanApplyTo(parameters);
}

interface IComponentFactory
{
    IComponent Create();
    bool CanApplyUpgradeEffect(IUpgradeEffect effect);
}
