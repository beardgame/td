using Bearded.Graphics;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class BoxShadowRenderer(IShapeDrawer drawer) : IRenderer<BoxShadow>
{
    public void Render(BoxShadow control)
    {
        var frame = control.Frame;

        var rectangle = Rectangle(frame.TopLeft, frame.Size, control.CornerRadius);
        var shadow = Shadow(control.Offset, control.BlurRadius, control.Color);

        drawer.DrawShadowFor(rectangle, shadow);
    }
}
