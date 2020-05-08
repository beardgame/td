using Bearded.TD.Game.Events;

namespace Bearded.TD.Game.Rules
{
    abstract class GameRule<TParameters> : IGameRule<GameState>
    {
        private readonly TParameters parameters;

        protected GameRule(TParameters parameters)
        {
            this.parameters = parameters;
        }

        public void OnAdded(GameState owner, GlobalGameEvents events)
        {
            RegisterEvents(events, parameters);
            Execute(owner, parameters);
        }

        protected virtual void RegisterEvents(GlobalGameEvents events, TParameters parameters)
        {
        }

        protected virtual void Execute(GameState gameState, TParameters parameters)
        {
        }
    }

    abstract class GameRule : GameRule<VoidParameters>
    {
        protected GameRule() : base(null)
        {
        }

        protected sealed override void RegisterEvents(GlobalGameEvents events, VoidParameters parameters) =>
            RegisterEvents(events);

        protected virtual void RegisterEvents(GlobalGameEvents events) {}

        protected sealed override void Execute(GameState owner, VoidParameters parameters) => Execute(owner);

        protected virtual void Execute(GameState gameState) {}
    }
}
