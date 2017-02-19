using amulware.Graphics;
using Bearded.Utilities.Math;
using OpenTK;

namespace Bearded.TD.Rendering
{
    abstract class ScreenLayer
    {
        private const float fovy = Mathf.PiOver2;
        private const float zNear = .1f;
        private const float zFar = 1024f;

        protected ViewportSize ViewportSize { get; private set; }

        public abstract Matrix4 ViewMatrix { get; }

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

        public void OnResize(ViewportSize newSize)
        {
            ViewportSize = newSize;
            OnViewportSizeChanged();
        }

        public virtual bool HandleInput(UpdateEventArgs args) => true;
        public abstract void Update(UpdateEventArgs args);
        public abstract void Draw();
        protected virtual void OnViewportSizeChanged() { }
    }
}
