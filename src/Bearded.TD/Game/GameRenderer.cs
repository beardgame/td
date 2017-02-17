using Bearded.TD.Rendering;

namespace Bearded.TD.Game
{
    class GameRenderer
    {
        private readonly GameState state;
        private readonly GeometryManager geometries;

        public GameRenderer(GameState state, GeometryManager geometries)
        {
            this.state = state;
            this.geometries = geometries;
        }

        public void Draw()
        {
            foreach (var obj in state.GameObjects)
            {
                obj.Draw(geometries);
            }
        }
    }
}
