using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.Game
{
    class GameScreenLayer : ScreenLayer
    {
        private readonly GameState state;
        private readonly GeometryManager geometries;

        public GameScreenLayer(GameState state, GeometryManager geometries)
        {
            this.state = state;
            this.geometries = geometries;
        }

        public override void Draw()
        {
            foreach (var obj in state.GameObjects)
            {
                obj.Draw(geometries);
            }
        }

        public override Matrix4 GetViewMatrix()
        {
            throw new System.NotImplementedException();
        }
    }
}
