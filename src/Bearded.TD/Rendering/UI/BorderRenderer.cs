using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

sealed class BorderRenderer(IShapeDrawer drawer) : IRenderer<Border>
{
    public void Render(Border control)
    {
        var frame = control.Frame;

        var shape = Shapes.Shapes.Rectangle(frame.TopLeft, frame.Size);
        var components = new ShapeComponents(Edge: Edge.Inner(1, control.Color)).ForDrawingAssumingNoGradients();

        drawer.Draw(shape, components);
    }
}
