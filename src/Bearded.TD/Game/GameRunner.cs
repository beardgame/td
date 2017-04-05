using amulware.Graphics;
using Bearded.TD.Game.UI;
using Bearded.TD.Utilities.Input;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameRunner
    {
        private readonly GameInstance game;
        private readonly InputManager inputManager;
        private readonly GameController controller;

        public GameRunner(GameInstance game, InputManager inputManager)
        {
            this.game = game;
            this.inputManager = inputManager;
            controller = new GameController(game);
        }

        public void HandleInput(UpdateEventArgs args)
        {
            controller.Update(PlayerInput.Construct(inputManager, game.Camera));
            game.Camera.Update(args.ElapsedTimeInSf);
        }

        public void Update(UpdateEventArgs args)
        {
            if (game.State.Meta.GameOver) return;
            var elapsedTime = new TimeSpan(args.ElapsedTimeInS);
            game.State.Resources.DistributeResources(elapsedTime);
            game.State.Navigator.Update();
            game.State.Advance(elapsedTime);
        }
    }
}
