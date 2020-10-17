using amulware.Graphics;
using amulware.Graphics.Shapes;
using amulware.Graphics.Text;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using Bearded.Utilities;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    sealed class UIDebugOverlayHighlightRenderer : IRenderer<UIDebugOverlayControl.Highlight>
    {
        private readonly IShapeDrawer2<Color> shapeDrawer;
        private readonly TextDrawerWithDefaults<Color> textDrawer;

        public UIDebugOverlayHighlightRenderer(
            IShapeDrawer2<Color> shapeDrawer, TextDrawerWithDefaults<Color> textDrawer)
        {
            this.shapeDrawer = shapeDrawer;
            this.textDrawer = textDrawer.With(fontHeight: 14);
        }

        public void Render(UIDebugOverlayControl.Highlight control)
        {
            var argb = Color.IndianRed * control.Alpha;
            shapeDrawer.DrawRectangle((Vector2)control.Frame.TopLeft, (Vector2)control.Frame.Size, 1, argb);

            var xy = new Vector2d(control.Frame.X.Start, control.TextY);
            textDrawer.DrawLine(
                xyz: ((Vector2) xy).WithZ(),
                text: control.Name,
                parameters: argb);
        }
    }
}
