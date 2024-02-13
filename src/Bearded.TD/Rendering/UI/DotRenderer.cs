using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

sealed class DotRenderer(IShapeDrawer drawer) : IRenderer<Dot>
{
    public void Render(Dot control)
    {
        var frame = control.Frame;

        drawer.DrawCircle(
            frame.TopLeft + frame.Size * 0.5,
            frame.Size.X * 0.5,
            control.Color);
    }
}
