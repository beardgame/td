using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Utilities.Input;
using Bearded.UI.Navigation;

namespace Bearded.TD.UI.Controls
{
    class GameWorld : UpdateableNavigationNode<(GameInstance game, GameRunner runner)>
    {
        public GameInstance Game { get; private set; }
        private GameRunner runner;

        protected override void Initialize(
            DependencyResolver dependencies, (GameInstance game, GameRunner runner) parameters)
        {
            base.Initialize(dependencies, parameters);
            
            Game = parameters.game;
            runner = parameters.runner;
        }
        
        // todo: hook up these methods below

        public void HandleInput(UpdateEventArgs args, InputState inputState)
        {
            runner.HandleInput(args, inputState);
        }

        public override void Update(UpdateEventArgs args)
        {
            runner.Update(args);
        }
    }
}
