using amulware.Graphics;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.TD.Rendering.UI
{
    class BoxRenderer : IRenderer<Control>
    {
        private readonly Color color;
        private readonly PrimitiveGeometry geometry;

        public BoxRenderer(IndexedSurface<PrimitiveVertexData> surface, Color color)
        {
            this.color = color;
            geometry = new PrimitiveGeometry(surface)
            {
                Color = color,
                LineWidth = 1
            };
        }

        public virtual void Render(Control control)
        {
            var frame = control.Frame;

            geometry.Color = control.IsFocused ? Color.Gold : color;

            geometry.DrawRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, false);
        }
    }
}
