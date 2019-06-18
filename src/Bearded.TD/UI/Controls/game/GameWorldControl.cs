using System;
using amulware.Graphics;
using Bearded.TD.Game;
using Bearded.TD.Meta;
using Bearded.TD.Rendering;
using Bearded.TD.Rendering.Deferred;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using Bearded.Utilities.Geometry;
using OpenTK;
using static Bearded.TD.Constants.Game.World;
using MouseEventArgs = Bearded.UI.EventArgs.MouseEventArgs;

namespace Bearded.TD.UI.Controls
{
    class GameWorldControl : DefaultProjectionRenderLayerControl, IDeferredRenderLayer
    {
        private readonly GameInstance game;
        private readonly GeometryManager geometries;
        private readonly LevelGeometryManager levelGeometry;

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

            // TODO: this should not stay hardcoded forever
            var levelMaterial = game.Blueprints.Materials["default"];

            levelGeometry = new LevelGeometryManager(game, renderContext, levelMaterial);

            DeferredSurfaces = new ContentSurfaceManager(
                levelGeometry,
                game.Blueprints.Sprites
                );
        }

        public override void Draw()
        {
            geometries.ConsoleFont.SizeCoefficient = new Vector2(1, -1);

            var state = game.State;

            game.PlayerCursors.DrawCursors(geometries);
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
                radius * 10, Color.White * 0.25f
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
            if (UserSettings.Instance.Debug.Coordinates > 0)
            {
                drawDebugCoordinates(state);
                return;
            }

            var debugPathfinding = UserSettings.Instance.Debug.Pathfinding;
            if (debugPathfinding > 0)
            {
                state.Navigator.DrawDebug(geometries, debugPathfinding > 1);
            }
        }

        private void drawDebugCoordinates(GameState state)
        {
            var geo = geometries.ConsoleFont;
            geo.Height = .3f * HexagonSide;
            geo.Color = Color.Beige;

            var topLeftOffset = Direction2.FromDegrees(150).Vector * HexagonSide * .6f;
            var bottomOffset = Direction2.FromDegrees(270).Vector * HexagonSide * .6f;
            var topRightOffset = Direction2.FromDegrees(30).Vector * HexagonSide * .6f;

            foreach (var tile in new Tilemap<Bearded.Utilities.Void>(state.Level.Radius))
            {
                if (UserSettings.Instance.Debug.Coordinates > 1)
                {
                    var (x, y, z) = tile.ToXYZ();

                    var tilePos = Level.GetPosition(tile).NumericValue;
                    var topLeft = tilePos + topLeftOffset;
                    var bottom = tilePos + bottomOffset;
                    var topRight = tilePos + topRightOffset;

                    if (x == 0 && y == 0 && z == 0)
                    {
                        geo.DrawString(topLeft, "x", .5f, .5f);
                        geo.DrawString(bottom, "y", .5f, .5f);
                        geo.DrawString(topRight, "z", .5f, .5f);
                    }
                    else
                    {
                        geo.DrawString(topLeft, $"{x:+0;-0;0}", .5f, .5f);
                        geo.DrawString(bottom, $"{y:+0;-0;0}", .5f, .5f);
                        geo.DrawString(topRight, $"{z:+0;-0;0}", .5f, .5f);
                    }
                }
                else
                {
                    geo.DrawString(Level.GetPosition(tile).NumericValue, $"{tile.X}, {tile.Y}", .5f, .5f);
                }
            }
        }

        public override void MouseEntered(MouseEventArgs eventArgs)
        {
            base.MouseEntered(eventArgs);
            game.PlayerInput.Focus();
        }

        public override void MouseExited(MouseEventArgs eventArgs)
        {
            base.MouseExited(eventArgs);
            game.PlayerInput.UnFocus();
        }

        public void CleanUp()
        {
            levelGeometry.CleanUp();
            new GraphicsUnloader().CleanUp(game.Blueprints);
        }
    }
}
