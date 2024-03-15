using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.TD.UI.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

class BoxRenderer(IShapeDrawer drawer, Color color) : IRenderer<Control>
{
    public virtual void Render(Control control)
    {
        var frame = control.Frame;

        var shape = Shapes.Shapes.Rectangle(frame.TopLeft, frame.Size);
        var components = new ShapeComponents(Fill: color).ForDrawingAssumingNoGradients();

        drawer.Draw(shape, components);
    }
}
