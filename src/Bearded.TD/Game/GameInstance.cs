namespace Bearded.TD.Game
{
    class GameInstance
    {
        public GameState State { get; }
        public GameCamera Camera { get; }

        // Bad!
        public int SelectedClickHandler = -1;

        public GameInstance(GameState state, GameCamera camera)
        {
            State = state;
            Camera = camera;
        }
    }
}
