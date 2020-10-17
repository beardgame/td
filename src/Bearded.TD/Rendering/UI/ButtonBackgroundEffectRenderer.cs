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
    sealed class ButtonBackgroundEffectRenderer : IRenderer<ButtonBackgroundEffect>
    {
        private readonly ShapeDrawer2<ColorVertexData, Color> drawer;

        public ButtonBackgroundEffectRenderer(ShapeDrawer2<ColorVertexData, Color> drawer)
        {
            this.drawer = drawer;
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
