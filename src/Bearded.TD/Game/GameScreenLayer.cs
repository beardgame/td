using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.Utilities.Math;
using OpenTK;

namespace Bearded.TD.Game
{
    class GameScreenLayer : ScreenLayer
    {
        private readonly GameState state;
        private readonly GameCamera camera;
        private readonly GeometryManager geometries;

        public GameScreenLayer(GameState state, GameCamera camera, GeometryManager geometries)
        {
            this.state = state;
            this.camera = camera;
            this.geometries = geometries;
        }

        public override void Draw()
        {
            geometries.ConsoleBackground.Color = Color.Red;
            geometries.ConsoleBackground.DrawCircle(Vector2.Zero, .5f, true, 6);

            foreach (var obj in state.GameObjects)
            {
                obj.Draw(geometries);
            }
        }

        public override Matrix4 ViewMatrix
        {
            get
            {
                var eye = camera.CameraPosition.WithZ(camera.CameraDistance);
                var target = camera.CameraPosition.WithZ(0);
                return Matrix4.LookAt(eye, target, Vector3.UnitY);
            }
        }
    }
}
