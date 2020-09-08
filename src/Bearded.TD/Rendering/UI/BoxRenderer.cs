using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    class BoxRenderer : IRenderer<Control>
    {
        private readonly Color color;
        private readonly ColorShapeDrawer2 drawer;

        public BoxRenderer(IIndexedTrianglesMeshBuilder<ColorVertexData, ushort> meshBuilder, Color color)
        {
            this.color = color;
            drawer = new ColorShapeDrawer2(meshBuilder);
        }

        public virtual void Render(Control control)
        {
            var frame = control.Frame;

            drawer.DrawRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, color, 1);
        }
    }
}
