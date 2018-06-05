using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.UI;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.UI.Controls
{
    class GameWorldView : RenderLayerCompositeControl
    {
        private const float fovy = Mathf.PiOver2;
        private const float zNear = .1f;
        private const float zFar = 1024f;

        private readonly GameInstance game;
        private readonly GeometryManager geometries;

        private ViewportSize viewportSize;

        public override Matrix4 ViewMatrix => game.Camera.ViewMatrix;
        public override Matrix4 ProjectionMatrix
        {
            get
            {
                var yMax = zNear * Mathf.Tan(.5f * fovy);
                var yMin = -yMax;
                var xMax = yMax * viewportSize.AspectRatio;
                var xMin = yMin * viewportSize.AspectRatio;
                return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
            }
        }
        public override RenderOptions RenderOptions => RenderOptions.Game;

        public GameWorldView(GameInstance game, FrameCompositor compositor, GeometryManager geometryManager)
            : base(compositor)
        {
            this.game = game;
            geometries = geometryManager;
        }

        public override void Draw()
        {
            updateViewport();

            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, -1);

            var state = game.State;
            
            state.Level.Draw(geometries);
            drawAmbientLight(state);
            drawGameObjects(state);
            drawDebug(state);
        }

        private void updateViewport()
        {
            var frame = Frame;
            viewportSize = new ViewportSize((int) frame.Size.X, (int) frame.Size.Y);
            game.Camera.OnViewportSizeChanged(viewportSize);
        }

        private void drawAmbientLight(GameState state)
        {
            var radius = state.Level.Tilemap.Radius;

            geometries.PointLight.Draw(
                new Vector3(-radius * 2, radius * 2, radius),
                radius * 10, Color.White * 0.15f
                );
        }

        private void drawGameObjects(GameState state)
        {
            foreach (var obj in state.GameObjects)
            {
                obj.Draw(geometries);
            }
        }

        private void drawDebug(GameState state)
        {
            var debugPathfinding = UserSettings.Instance.Debug.Pathfinding;
            if (debugPathfinding > 0)
            {
                state.Navigator.DrawDebug(geometries, state.Level, debugPathfinding > 1);
            }
        }
    }
}
