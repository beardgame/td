using Bearded.TD.Game.Simulation.Events;

namespace Bearded.TD.Game.Simulation.Rules
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
            RegisterEvents(events, owner.GameSettings, parameters);
            Execute(owner, parameters);
        }

        protected virtual void RegisterEvents(
            GlobalGameEvents events, GameSettings gameSettings, TParameters parameters)
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

        protected sealed override void RegisterEvents(
                GlobalGameEvents events, GameSettings gameSettings, VoidParameters parameters) =>
            RegisterEvents(events, gameSettings);

        protected virtual void RegisterEvents(GlobalGameEvents events, GameSettings gameSettings) {}

        protected sealed override void Execute(GameState owner, VoidParameters parameters) => Execute(owner);

        protected virtual void Execute(GameState gameState) {}
    }
}
