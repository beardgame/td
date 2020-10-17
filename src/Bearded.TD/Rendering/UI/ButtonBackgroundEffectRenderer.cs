using amulware.Graphics;
using amulware.Graphics.MeshBuilders;
using amulware.Graphics.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenToolkit.Mathematics;
using ColorVertexData = Bearded.TD.Rendering.Vertices.ColorVertexData;

namespace Bearded.TD.Rendering.UI
{
    class ButtonBackgroundEffectRenderer : IRenderer<ButtonBackgroundEffect>
    {
        private readonly ColorShapeDrawer2 drawer;

        public ButtonBackgroundEffectRenderer(IIndexedTrianglesMeshBuilder<ColorVertexData, ushort> meshBuilder)
        {
            drawer = new ColorShapeDrawer2(meshBuilder);
        }

        public void Render(ButtonBackgroundEffect control)
        {
            if (!control.MouseIsOver)
                return;

            if (control.Parent is Button button && !button.IsEnabled)
                return;

            var frame = control.Frame;

            var color = Color.White * (control.MouseIsDown ? 0.5f : 0.25f);

            drawer.FillRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, color);
        }
    }
}
