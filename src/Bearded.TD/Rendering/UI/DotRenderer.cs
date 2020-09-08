using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    sealed class DotRenderer : IRenderer<Dot>
    {
        private readonly ColorShapeDrawer2 drawer;

        public DotRenderer(IIndexedTrianglesMeshBuilder<ColorVertexData, ushort> meshBuilder)
        {
            drawer = new ColorShapeDrawer2(meshBuilder);
        }

        public void Render(Dot control)
        {
            var frame = control.Frame;

            drawer.FillOval((Vector2) frame.TopLeft, (Vector2) frame.Size, control.Color, 6);
        }
    }
}
