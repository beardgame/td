using amulware.Graphics;
using amulware.Graphics.Shapes;
using Bearded.TD.Meta;
using Bearded.UI.Controls;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering.UI
{
    sealed class FallbackBoxRenderer : BoxRenderer
    {
        public FallbackBoxRenderer(ShapeDrawer2<ColorVertexData, Color> drawer)
            : base(drawer, Color.Fuchsia)
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
