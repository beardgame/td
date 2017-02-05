using amulware.Graphics;
using Bearded.Utilities.SpaceTime;

namespace Bearded.TD
{
    internal class GameRunner
    {
        private readonly GameState state;

        public GameRunner(GameState state)
        {
            this.state = state;
        }

        public void Update(UpdateEventArgs args)
        {
            state.Advance(new TimeSpan(args.ElapsedTimeInS));
        }
    }
}