using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.Game
{
    class GameScreenLayer : ScreenLayer
    {
        private readonly GameState state;
        private readonly GameCamera camera;
        private readonly GeometryManager geometries;

        public override Matrix4 ViewMatrix => camera.ViewMatrix;

        public GameScreenLayer(GameState state, GameCamera camera, GeometryManager geometries)
        {
            this.state = state;
            this.camera = camera;
            this.geometries = geometries;
        }

        public override void Draw()
        {
            state.Level.Draw(geometries);

            foreach (var obj in state.GameObjects)
            {
                obj.Draw(geometries);
            }
        }

        protected override void OnViewportSizeChanged()
        {
            base.OnViewportSizeChanged();
            camera.OnViewportSizeChanged(ViewportSize);
        }
    }
}
