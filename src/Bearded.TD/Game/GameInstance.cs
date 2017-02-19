using Bearded.TD.Game.UI;

namespace Bearded.TD.Game
{
    class GameInstance
    {
        public GameState State { get; }
        public GameCamera Camera { get; }
        public CursorState Cursor { get; }

        public GameInstance(GameState state, GameCamera camera)
        {
            State = state;
            Camera = camera;
            Cursor = new CursorState(state);
        }
    }
}
