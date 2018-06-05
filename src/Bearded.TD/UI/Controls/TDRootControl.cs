using Bearded.TD.Rendering;
using OpenTK;

namespace Bearded.TD.UI.Controls
{
    sealed class TDRootControl : DefaultProjectionRenderLayerView
    {
        public override Matrix4 ViewMatrix
        {
            get
            {
                var originCenter = new Vector3(
                    .5f * ViewportSize.ScaledWidth,
                    .5f * ViewportSize.ScaledHeight,
                    0);
                var eye = new Vector3(0, 0, -.5f * ViewportSize.ScaledHeight) + originCenter;
                return Matrix4.LookAt(
                    eye,
                    originCenter,
                    -Vector3.UnitY);
            }
        }

        public override RenderOptions RenderOptions { get; } = RenderOptions.Default;

        public TDRootControl(FrameCompositor compositor) : base(compositor) { }
    }
}
