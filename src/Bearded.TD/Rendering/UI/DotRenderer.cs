using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Rendering;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class DotRenderer(IShapeDrawer drawer, IShapeComponentBuffer componentBuffer) : IRenderer<Dot>
{
    public void Render(Dot control)
    {
        var frame = control.Frame;

        var shape = Circle(frame.TopLeft + frame.Size * 0.5, frame.Size.X * 0.5);
        var components = ShapeComponentsForDrawing.From(Fill.With(control.Color), componentBuffer);

        drawer.Draw(shape, components);
    }
}
