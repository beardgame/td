using amulware.Graphics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD.Game
{
    class GameRunner
    {
        private readonly GameState state;
        private readonly GameCamera camera;

        public GameRunner(GameState state, GameCamera camera)
        {
            this.state = state;
            this.camera = camera;
        }

        public void Update(UpdateEventArgs args)
        {
            camera.Update(args.ElapsedTimeInSf);
            state.Advance(new TimeSpan(args.ElapsedTimeInS));
        }
    }
}
