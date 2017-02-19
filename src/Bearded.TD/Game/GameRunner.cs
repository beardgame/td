using amulware.Graphics;
using Bearded.TD.Game.Interaction;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameRunner
    {
        private readonly GameState state;
        private readonly GameCamera camera;
        private readonly GameController controller;

        public GameRunner(GameState state, GameCamera camera)
        {
            this.state = state;
            this.camera = camera;
            controller = new GameController(state);
        }

        public void Update(UpdateEventArgs args)
        {
            var elapsedTime = new TimeSpan(args.ElapsedTimeInS);

            camera.Update(args.ElapsedTimeInSf);
            state.Navigator.Update();
            controller.Update(elapsedTime, PlayerInput.Construct(camera));
            state.Advance(elapsedTime);
        }
    }
}
