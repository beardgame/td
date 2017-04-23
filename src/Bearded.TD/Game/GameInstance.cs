using System;
using Bearded.TD.Commands;
using Bearded.TD.Game.Players;
using Bearded.TD.Game.UI;
using Bearded.TD.Utilities;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public Player Me { get; }
        public IRequestDispatcher RequestDispatcher { get; }
        
        public ChatLog ChatLog { get; } = new ChatLog();
        public IdManager Ids { get; } = new IdManager();

        public GameState State { get; private set; }
        public GameCamera Camera { get; private set; }
        public CursorState Cursor { get; private set; }

        public GameInstance(Player me, IRequestDispatcher requestDispatcher)
        {
            Me = me;
            RequestDispatcher = requestDispatcher;
        }

        public void Start(GameState state, GameCamera camera)
        {
            if (State != null)
                throw new Exception();

            State = state;
            Camera = camera;
            Cursor = new CursorState(this); // bad.
        }
    }
}
