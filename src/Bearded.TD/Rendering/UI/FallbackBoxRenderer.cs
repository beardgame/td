using amulware.Graphics;
using Bearded.TD.Meta;
using Bearded.UI.Controls;

namespace Bearded.TD.Rendering.UI
{
    sealed class FallbackBoxRenderer : BoxRenderer
    {
        public FallbackBoxRenderer(IndexedSurface<PrimitiveVertexData> surface)
            : base(surface, Color.Fuchsia)
        {
        }

        public override void Render(Control control)
        {
            if (!UserSettings.Instance.Debug.RenderUIFallBack)
                return;

            base.Render(control);
        }
    }
}
