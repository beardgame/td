using Bearded.TD.Rendering;

namespace Bearded.TD.Game
{
    class GameRenderer
    {
        private readonly GameState state;
        private readonly SpriteManager sprites;

        public GameRenderer(GameState state, SpriteManager sprites)
        {
            this.state = state;
            this.sprites = sprites;
        }

        public void Draw()
        {
            foreach (var obj in state.GameObjects)
            {
                obj.Draw(sprites);
            }
        }
    }
}
