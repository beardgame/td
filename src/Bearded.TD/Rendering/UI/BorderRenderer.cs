using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

sealed class BorderRenderer(IShapeDrawer drawer, IShapeComponentBuffer shapeComponents) : IRenderer<Border>
{
    public void Render(Border control)
    {
        var frame = control.Frame;

        var shape = Shapes.Shapes.Rectangle(frame.TopLeft, frame.Size);
        var components = ShapeComponentsForDrawing.From(Edge.Inner(1, control.Color), shapeComponents);

        drawer.Draw(shape, components);
    }
}
