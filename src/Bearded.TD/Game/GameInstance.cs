using Bearded.TD.Commands;
using Bearded.TD.Game.UI;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public GameState State { get; }
        public GameCamera Camera { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        public CursorState Cursor { get; }

        public GameInstance(GameState state, GameCamera camera, IRequestDispatcher requestDispatcher)
        {
            State = state;
            Camera = camera;
            RequestDispatcher = requestDispatcher;
            Cursor = new CursorState(state);
        }
    }
}
