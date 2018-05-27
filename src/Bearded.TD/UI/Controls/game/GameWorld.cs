
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Utilities.Input;

namespace Bearded.TD.UI.Controls
{
    class GameWorld
    {
        // todo: inject these
        public GameInstance Game { get; }
        private readonly GameRunner runner;
        
        // todo: hook up these methods below

        public void HandleInput(UpdateEventArgs args, InputState inputState)
        {
            runner.HandleInput(args, inputState);
        }

        public void Update(UpdateEventArgs args)
        {
            runner.Update(args);
        }

    }
}
