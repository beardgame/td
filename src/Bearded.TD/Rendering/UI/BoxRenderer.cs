using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.Rendering.UI
{
    class BoxRenderer : IRenderer<Control>
    {
        private readonly PrimitiveGeometry geometry;

        public BoxRenderer(IndexedSurface<PrimitiveVertexData> surface, Color color)
        {
            geometry = new PrimitiveGeometry(surface)
            {
                Color = color,
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
