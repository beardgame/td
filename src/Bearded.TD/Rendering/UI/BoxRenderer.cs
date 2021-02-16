using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    class BoxRenderer : IRenderer<Control>
    {
        private readonly IShapeDrawer2<Color> drawer;
        private readonly Color color;

        public BoxRenderer(IShapeDrawer2<Color> drawer, Color color)
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
