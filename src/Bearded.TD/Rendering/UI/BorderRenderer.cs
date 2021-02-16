using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    sealed class BorderRenderer : IRenderer<Border>
    {
        private readonly IShapeDrawer2<Color> drawer;

        public BorderRenderer(IShapeDrawer2<Color> drawer)
        {
            this.drawer = drawer;
        }

        public void Render(Border control)
        {
            var frame = control.Frame;

            drawer.DrawRectangle((Vector2)frame.TopLeft, (Vector2)frame.Size, 1, control.Color);
        }
    }
}
