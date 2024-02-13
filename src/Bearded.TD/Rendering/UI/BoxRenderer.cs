using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

class BoxRenderer(IShapeDrawer drawer, Color color) : IRenderer<Control>
{
    public virtual void Render(Control control)
    {
        var frame = control.Frame;

        drawer.DrawRectangle(frame.TopLeft, frame.Size, new ShapeColors(fill: color));
    }
}
