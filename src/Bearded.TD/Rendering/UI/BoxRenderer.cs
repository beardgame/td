using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering.UI
{
    class BoxRenderer : IRenderer<Control>
    {
        private readonly ShapeDrawer2<ColorVertexData, Color> drawer;
        private readonly Color color;

        public BoxRenderer(ShapeDrawer2<ColorVertexData, Color> drawer, Color color)
        {
            this.drawer = drawer;
            this.color = color;
        }

        public virtual void Render(Control control)
        {
            var frame = control.Frame;

            drawer.DrawRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, 1, color);
        }
    }
}
