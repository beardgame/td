using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Rendering;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class BackgroundBoxRenderer(IShapeDrawer drawer) : IRenderer<BackgroundBox>
{
    public void Render(BackgroundBox control)
    {
        var frame = control.Frame;

        var shape = Rectangle(frame.TopLeft, frame.Size);
        var components = new ShapeComponents(Fill: control.Color).ForDrawingAssumingNoGradients();

        drawer.Draw(shape, components);
    }
}
