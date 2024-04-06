using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class DropShadowRenderer(IShapeDrawer drawer, IShapeComponentBuffer shapeBuffer, IGradientBuffer gradientBuffer)
    : IRenderer<DropShadow>
{
    public void Render(DropShadow control)
    {
        var frame = control.Frame;
        var (tl, size) = (frame.TopLeft, frame.Size);

        var shadow = new ShadowForDrawing(control.Shadow, shapeBuffer, control.OverlayComponents, (gradientBuffer, frame));

        switch (control.SourceControl)
        {
            case ComplexCircle { Shape: var circle }:
            {
                drawer.DrawShadowFor(circle, shadow);
                break;
            }
            case ComplexHexagon { Shape: var hexagon }:
            {
                drawer.DrawShadowFor(hexagon, shadow);
                break;
            }
            default:
            {
                drawer.DrawShadowFor(Rectangle(tl, size, control.CornerRadius), shadow);
                break;
            }
        }
    }
}
