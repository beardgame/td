using Bearded.TD.Rendering.Shapes;
using Bearded.TD.UI.Controls;
using Bearded.UI.Rendering;
using static System.Math;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.UI;

sealed class ComplexShapeRenderer(IShapeDrawer shapes) : IRenderer<ComplexShapeControl>
{
    public void Render(ComplexShapeControl control)
    {
        var frame = control.Frame;

        switch (control)
        {
            case ComplexBox:
            {
                shapes.Draw(Rectangle(frame.TopLeft, frame.Size, control.CornerRadius), control.Colors, control.Edges);
                break;
            }
            case ComplexCircle:
            {
                var center = frame.TopLeft + frame.Size * 0.5;
                var radius = Min(frame.Size.X, frame.Size.Y) * 0.5;

                shapes.Draw(Circle(center, radius), control.Colors, control.Edges);
                break;
            }
        }
    }
}
