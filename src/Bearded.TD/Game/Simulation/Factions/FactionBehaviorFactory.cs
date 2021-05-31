using System;

namespace Bearded.TD.Game.Simulation.Factions
{
    interface IFactionBehaviorFactory<TOwner>
    {
        FactionBehavior<TOwner> Create();
    }

    sealed class FactionBehaviorFactory<TOwner, TParameters> : IFactionBehaviorFactory<TOwner>
    {
        private readonly TParameters parameters;
        private readonly Func<TParameters, FactionBehavior<TOwner>> factory;

        public FactionBehaviorFactory(TParameters parameters, Func<TParameters, FactionBehavior<TOwner>> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public FactionBehavior<TOwner> Create() => factory(parameters);
    }
}
