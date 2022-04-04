using System;

namespace Bearded.TD.Game.Simulation.Factions;

interface IFactionBehaviorFactory
{
    IFactionBehavior Create();
}

sealed class FactionBehaviorFactory<TParameters> : IFactionBehaviorFactory
{
    private readonly TParameters parameters;
    private readonly Func<TParameters, IFactionBehavior> factory;

    public FactionBehaviorFactory(TParameters parameters, Func<TParameters, IFactionBehavior> factory)
    {
        this.parameters = parameters;
        this.factory = factory;
    }

    public IFactionBehavior Create() => factory(parameters);
}
