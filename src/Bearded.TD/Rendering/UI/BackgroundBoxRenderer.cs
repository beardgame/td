using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering.UI
{
    sealed class BackgroundBoxRenderer : IRenderer<BackgroundBox>
    {
        private readonly ShapeDrawer2<ColorVertexData, Color> drawer;

        public BackgroundBoxRenderer(ShapeDrawer2<ColorVertexData, Color> drawer)
        {
            this.drawer = drawer;
        }

        public void Render(BackgroundBox control)
        {
            var frame = control.Frame;

            drawer.FillRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, control.Color);
        }
    }
}
