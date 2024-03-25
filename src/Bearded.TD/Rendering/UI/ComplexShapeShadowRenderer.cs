using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class DropShadowRenderer(IShapeDrawer drawer) : IRenderer<DropShadow>
{
    public void Render(DropShadow control)
    {
        var frame = control.Frame;
        var (tl, size) = (frame.TopLeft, frame.Size);

        switch (control.SourceControl)
        {
            case ComplexCircle { Shape: var circle }:
            {
                drawer.DrawShadowFor(circle, control.Shadow);
                break;
            }
            case ComplexHexagon { Shape: var hexagon }:
            {
                drawer.DrawShadowFor(hexagon, control.Shadow);
                break;
            }
            default:
            {
                drawer.DrawShadowFor(Rectangle(tl, size, control.CornerRadius), control.Shadow);
                break;
            }
        }
    }
}
