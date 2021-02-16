using Bearded.Graphics;
using Bearded.Graphics.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.UI
{
    sealed class DotRenderer : IRenderer<Dot>
    {
        private readonly IShapeDrawer2<Color> drawer;

        public DotRenderer(IShapeDrawer2<Color> drawer)
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
