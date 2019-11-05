using System;
using Bearded.TD.Game;
using Bearded.TD.Rendering;
using Bearded.TD.UI.Layers;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using OpenTK;
using MouseEventArgs = Bearded.UI.EventArgs.MouseEventArgs;

namespace Bearded.TD.UI.Controls
{
    class GameWorldControl : DefaultProjectionRenderLayerControl, IDeferredRenderLayer
    {
        private readonly GameRenderer renderer;
        
        private readonly GameInstance game;

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

        public float Time => (float)game.State.Time.NumericValue;
        public ContentSurfaceManager DeferredSurfaces => renderer.DeferredSurfaces;

        public GameWorldControl(GameInstance game, RenderContext renderContext)
        {
            this.game = game;
            renderer = new GameRenderer(game, renderContext);
        }

        public override void Draw()
        {
            renderer.Render();
        }

        public override void UpdateViewport(ViewportSize viewport)
        {
            base.UpdateViewport(viewport);
            game.Camera.OnViewportSizeChanged(ViewportSize);
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
            renderer.CleanUp();
        }
    }
}
