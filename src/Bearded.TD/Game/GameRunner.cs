using amulware.Graphics;
using Bearded.TD.Game.Interaction;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameRunner
    {
        private readonly GameInstance game;
        private readonly GameController controller;

        public GameRunner(GameInstance game)
        {
            this.game = game;
            controller = new GameController(game);
        }

        public void HandleInput(UpdateEventArgs args)
        {
            controller.Update(PlayerInput.Construct(game.Camera));
            game.Camera.Update(args.ElapsedTimeInSf);
        }

        public void Update(UpdateEventArgs args)
        {
            var elapsedTime = new TimeSpan(args.ElapsedTimeInS);
            game.State.Navigator.Update();
            game.State.Advance(elapsedTime);
        }
    }
}
