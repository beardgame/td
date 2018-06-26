using Bearded.Utilities;
using OpenTK;

namespace Bearded.TD.UI.Layers
{
    abstract class DefaultProjectionRenderLayerControl : RenderLayerCompositeControl
    {
        private const float fovy = Mathf.PiOver2;
        private const float zNear = .1f;
        private const float zFar = 1024f;

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
    }
}