using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public Player Me { get; }
        public GameState State { get; }
        public GameCamera Camera { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        public CursorState Cursor { get; }
        public ChatLog ChatLog { get; } = new ChatLog();

        public GameInstance(Player me, GameState state, GameCamera camera, IRequestDispatcher requestDispatcher)
        {
            Me = me;
            State = state;
            Camera = camera;
            RequestDispatcher = requestDispatcher;
            Cursor = new CursorState(this); // bad.
        }
    }
}
