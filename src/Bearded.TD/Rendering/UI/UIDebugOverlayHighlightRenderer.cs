using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.Rendering.UI
{
    class UIDebugOverlayHighlightRenderer : IRenderer<UIDebugOverlayControl.Highlight>
    {
        private readonly PrimitiveGeometry background;
        private readonly FontGeometry text;

        public UIDebugOverlayHighlightRenderer(IndexedSurface<PrimitiveVertexData> backgroundSurface,
            IndexedSurface<UVColorVertexData> fontSurface, Font font)
        {
            background = new PrimitiveGeometry(backgroundSurface)
            {
                LineWidth = 1
            };
            text = new FontGeometry(fontSurface, font)
            {
                Height = 14
            };
        }

        public void Render(UIDebugOverlayControl.Highlight control)
        {
            var argb = Color.IndianRed * control.Alpha;
            background.Color = argb;
            background.DrawRectangle((Vector2)control.Frame.TopLeft, (Vector2)control.Frame.Size, false);

            text.Color = argb;
            text.DrawString((Vector2)new Vector2d(control.Frame.X.Start, control.TextY), control.Name);
        }
    }
}
