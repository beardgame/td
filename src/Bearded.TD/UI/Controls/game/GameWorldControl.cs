using System;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using OpenTK;
using MouseEventArgs = Bearded.UI.EventArgs.MouseEventArgs;

namespace Bearded.TD.UI.Controls
{
    class GameWorldControl : DefaultProjectionRenderLayerControl, IDeferredRenderLayer
    {
        private readonly GameInstance game;
        private readonly GeometryManager geometries;

        public override Matrix4 ViewMatrix => game.Camera.ViewMatrix;
        public override RenderOptions RenderOptions => RenderOptions.Default;

        private const float fovy = Mathf.PiOver2;
        private const float lowestZToRender = -10;
        private const float highestZToRender = 5;

        public override Matrix4 ProjectionMatrix
        {
            get
            {
                var zNear = Math.Max(game.Camera.Distance - highestZToRender, 0.1f);
                var zFar = FarPlaneDistance;

                var yMax = zNear * Mathf.Tan(.5f * fovy);
                var yMin = -yMax;
                var xMax = yMax * ViewportSize.AspectRatio;
                var xMin = yMin * ViewportSize.AspectRatio;
                return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
            }
        }

        public float FarPlaneDistance => game.Camera.Distance - lowestZToRender;
        public ContentSurfaceManager DeferredSurfaces { get; }

        public GameWorldControl(GameInstance game, RenderContext renderContext)
        {
            this.game = game;
            geometries = renderContext.Geometries;

            var levelGeometry = new LevelGeometryManager(game, renderContext);

            DeferredSurfaces = new ContentSurfaceManager(levelGeometry, game.Blueprints.Sprites);
        }

        public override void Draw()
        {
            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, -1);

            var state = game.State;
            
            drawAmbientLight(state);
            drawGameObjects(state);
            drawDebug(state);
        }

        public override void UpdateViewport(ViewportSize viewport)
        {
            base.UpdateViewport(viewport);
            game.Camera.OnViewportSizeChanged(ViewportSize);
        }

        private void drawAmbientLight(GameState state)
        {
            var radius = state.Level.Radius;

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

        public override void MouseMoved(MouseEventArgs eventArgs)
        {
            base.MouseMoved(eventArgs);
            game.PlayerInput.IsMouseFocused = true;
        }

        public override void MouseExited(MouseEventArgs eventArgs)
        {
            base.MouseExited(eventArgs);
            game.PlayerInput.IsMouseFocused = false;
        }
    }
}
