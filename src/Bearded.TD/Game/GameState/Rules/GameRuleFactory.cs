using System;

namespace Bearded.TD.Game.GameState.Rules
{
    interface IGameRuleFactory<in TOwner>
    {
        IGameRule<TOwner> Create();
    }

    sealed class GameRuleFactory<TOwner, TParameters> : IGameRuleFactory<TOwner>
    {
        private readonly TParameters parameters;
        private readonly Func<TParameters, IGameRule<TOwner>> factory;

        public GameRuleFactory(TParameters parameters, Func<TParameters, IGameRule<TOwner>> factory)
        {
            this.parameters = parameters;
            this.factory = factory;
        }

        public IGameRule<TOwner> Create() => factory(parameters);
    }
}
