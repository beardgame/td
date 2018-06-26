using Bearded.TD.Rendering;
using Bearded.TD.Rendering.UI;
using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.UI.Controls
{
    abstract class DefaultProjectionRenderLayerControl : RenderLayerCompositeControl
    {
        private const float fovy = Mathf.PiOver2;
        private const float zNear = .1f;
        private const float zFar = 1024f;

        protected ViewportSize ViewportSize { get; private set; }
        
        public override Matrix4 ProjectionMatrix
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

        protected DefaultProjectionRenderLayerControl(FrameCompositor compositor)
            : base(compositor)
        {
        }

        public override void Draw()
        {
            UpdateViewport();
            
            base.Draw();
        }

        protected virtual void UpdateViewport()
        {
            var frame = Frame;
            ViewportSize = new ViewportSize((int)frame.Size.X, (int)frame.Size.Y);
        }
    }
}