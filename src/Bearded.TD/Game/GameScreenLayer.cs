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
            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, -1);

            state.Level.Draw(geometries);
            state.Navigator.DrawDebug(geometries, state.Level);

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
