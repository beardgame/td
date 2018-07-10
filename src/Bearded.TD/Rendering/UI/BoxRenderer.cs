using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;
using static amulware.Graphics.Color;

namespace Bearded.TD.Rendering.UI
{
    sealed class BoxRenderer : IRenderer<Control>
    {
        private readonly PrimitiveGeometry geometry;

        public BoxRenderer(IndexedSurface<PrimitiveVertexData> surface)
        {
            geometry = new PrimitiveGeometry(surface)
            {
                Color = White,
                LineWidth = 1
            };
        }
        
        public void Render(Control control)
        {
            var frame = control.Frame;

            geometry.DrawRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, false);
        }
    }
}
