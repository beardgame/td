using amulware.Graphics;
using Bearded.TD.Rendering;
using Bearded.TD.UI;
using Bearded.Utilities.Math;
using OpenTK;

namespace Bearded.TD.Screens
{
    abstract class ScreenLayer : IScreenLayer
    {
        private const float fovy = Mathf.PiOver2;
        private const float zNear = .1f;
        private const float zFar = 1024f;

        protected readonly ScreenLayerCollection Parent;

        protected ViewportSize ViewportSize { get; private set; }
        protected bool Destroyed { get; private set; }

        public abstract Matrix4 ViewMatrix { get; }
        public virtual RenderOptions RenderOptions => RenderOptions.Default;

        public virtual Matrix4 ProjectionMatrix
        {
            get
            {
                var yMax = zNear * Mathf.Tan(.5f * fovy);
                var yMin = -yMax;
                var xMax = yMax * ViewportSize.AspectRatio;
                var xMin = yMin * ViewportSize.AspectRatio;
                return Matrix4.CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
            }
        }

        protected ScreenLayer(ScreenLayerCollection parent)
        {
            Parent = parent;
        }

        public void OnResize(ViewportSize newSize)
        {
            ViewportSize = newSize;
            OnViewportSizeChanged();
        }

        public virtual bool HandleInput(UpdateEventArgs args, InputState inputState) => true;
        public abstract void Update(UpdateEventArgs args);
        public abstract void Draw();
        protected virtual void OnViewportSizeChanged() { }

        public void Render(RenderContext context)
        {
            context.Compositor.RenderLayer(this);
        }

        protected void Destroy()
        {
            Parent.RemoveScreenLayer(this);
            Destroyed = true;
        }
    }
}
