using amulware.Graphics;
using amulware.Graphics.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering.UI
{
    sealed class DotRenderer : IRenderer<Dot>
    {
        private readonly ShapeDrawer2<ColorVertexData, Color> drawer;

        public DotRenderer(ShapeDrawer2<ColorVertexData, Color> drawer)
        {
            this.drawer = drawer;
        }

        public void Render(Dot control)
        {
            var frame = control.Frame;

            drawer.FillOval((Vector2) frame.TopLeft, (Vector2) frame.Size, control.Color, 6);
        }
    }
}
