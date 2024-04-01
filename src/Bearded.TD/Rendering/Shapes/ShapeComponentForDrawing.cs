using System.Runtime.InteropServices;
using Bearded.UI;

namespace Bearded.TD.Rendering.Shapes;

[StructLayout(LayoutKind.Sequential)]
readonly struct ShapeComponentForDrawing
{
    private readonly float zeroDistance;
    private readonly float oneDistance;
    private readonly GradientParameters gradient;

    private ShapeComponentForDrawing(float zeroDistance, float oneDistance, GradientParameters gradient)
    {
        this.zeroDistance = zeroDistance;
        this.oneDistance = oneDistance;
        this.gradient = gradient;
    }

    public static ShapeComponentForDrawing From(
        TD.UI.Shapes.ShapeComponent component, (IGradientBuffer, Frame)? gradientsInFrame = null)
    {
        var gradient = gradientsInFrame is var (gradients, frame)
            ? component.Color.ForDrawingWith(gradients, frame)
            : component.Color.ForDrawingWithoutGradients();

        return new ShapeComponentForDrawing(component.ZeroDistance, component.OneDistance, gradient);
    }
}
