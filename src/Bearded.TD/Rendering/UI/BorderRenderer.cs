using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

sealed class BorderRenderer(IShapeDrawer drawer) : IRenderer<Border>
{
    public void Render(Border control)
    {
        var frame = control.Frame;

        var edges = new EdgeData(0, 1, 0, 0);

        drawer.DrawRectangle(
            frame.TopLeft,
            frame.Size,
            control.Color,
            10,
            edges);
    }
}
