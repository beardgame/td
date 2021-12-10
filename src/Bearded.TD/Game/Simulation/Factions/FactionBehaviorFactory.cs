using System;

namespace Bearded.TD.Game.Simulation.Factions;

interface IFactionBehaviorFactory<in TOwner>
{
    IFactionBehavior<TOwner> Create();
}

sealed class FactionBehaviorFactory<TOwner, TParameters> : IFactionBehaviorFactory<TOwner>
{
    private readonly TParameters parameters;
    private readonly Func<TParameters, IFactionBehavior<TOwner>> factory;

    public FactionBehaviorFactory(TParameters parameters, Func<TParameters, IFactionBehavior<TOwner>> factory)
    {
        this.parameters = parameters;
        this.factory = factory;
    }

    public IFactionBehavior<TOwner> Create() => factory(parameters);
}