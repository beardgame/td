using amulware.Graphics;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.Rendering.UI
{
    class ButtenBackgroundEffectRenderer : IRenderer<ButtonBackgroundEffect>
    {
        private readonly PrimitiveGeometry geometry;

        public ButtenBackgroundEffectRenderer(IndexedSurface<PrimitiveVertexData> surface)
        {
            geometry = new PrimitiveGeometry(surface);
        }

        public void Render(ButtonBackgroundEffect control)
        {
            if (!control.MouseIsOver)
                return;
            
            if (control.Parent is Button button && !button.IsEnabled)
                return;

            var frame = control.Frame;

            geometry.Color = Color.White * (control.MouseIsDown ? 0.5f : 0.25f);

            geometry.DrawRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size);
        }
    }
}
