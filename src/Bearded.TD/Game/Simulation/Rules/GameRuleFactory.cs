using System;

namespace Bearded.TD.Game.Simulation.Rules;

interface IGameRuleFactory
{
    IGameRule Create();
}

sealed class GameRuleFactory<TParameters> : IGameRuleFactory
{
    private readonly TParameters parameters;
    private readonly Func<TParameters, IGameRule> factory;

    public GameRuleFactory(TParameters parameters, Func<TParameters, IGameRule> factory)
    {
        this.parameters = parameters;
        this.factory = factory;
    }

    public IGameRule Create() => factory(parameters);
}
