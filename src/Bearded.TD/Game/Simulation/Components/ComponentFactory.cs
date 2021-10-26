using System;
using Bearded.TD.Game.Simulation.Upgrades;
using Bearded.TD.Shared.TechEffects;

namespace Bearded.TD.Game.Simulation.Components
{
    sealed class ComponentFactory<TOwner, TComponentParameters> : IComponentFactory<TOwner>
        where TComponentParameters : IParametersTemplate<TComponentParameters>
    {
        private readonly TComponentParameters parameters;
        private readonly Func<TComponentParameters, IComponent<TOwner>> factory;

        public ComponentFactory(TComponentParameters parameters, Func<TComponentParameters, IComponent<TOwner>> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public IComponent<TOwner> Create() => factory(parameters);

        public bool CanApplyUpgradeEffect(IUpgradeEffect effect) => effect.CanApplyTo(parameters);
    }

    interface IComponentFactory<in TOwner>
    {
        IComponent<TOwner> Create();
        bool CanApplyUpgradeEffect(IUpgradeEffect effect);
    }
}
