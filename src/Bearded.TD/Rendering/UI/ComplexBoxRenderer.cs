using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;

namespace Bearded.TD.Rendering.UI;

sealed class ComplexBoxRenderer(IShapeDrawer drawer) : IRenderer<ComplexBox>
{
    public void Render(ComplexBox control)
    {
        var frame = control.Frame;

        drawer.DrawRectangle(frame.TopLeft, frame.Size, control.Colors, control.CornerRadius, control.Edges);
    }
}
