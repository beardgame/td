namespace Bearded.TD.Game
{
    class GameRenderer
    {
        private readonly GameState state;

        public GameRenderer(GameState state)
        {
            this.state = state;
        }

        public void Draw()
        {
            foreach (var obj in state.GameObjects)
            {
                obj.Draw();
            }
        }
    }
}
