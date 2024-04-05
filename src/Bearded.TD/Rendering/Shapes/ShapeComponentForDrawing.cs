using System.Runtime.InteropServices;
using Bearded.TD.UI.Shapes;
using Bearded.UI;
using JetBrains.Annotations;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Shapes;

[StructLayout(LayoutKind.Sequential)]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
readonly struct ShapeComponentForDrawing
{
    // texel 1
    private readonly float zeroDistance;
    private readonly float oneDistance;
    private readonly uint type1gradientIndex3;
    private readonly uint flags;
    // texel 2
    private readonly Vector4 parameters;

    private ShapeComponentForDrawing(float zeroDistance, float oneDistance, GradientParameters gradient)
    {
        this.zeroDistance = zeroDistance;
        this.oneDistance = oneDistance;

        type1gradientIndex3 = (uint)gradient.Type | (gradient.Id.Value << 8);
        flags = (uint)gradient.Flags;
        parameters = gradient.Parameters;
    }

    public static ShapeComponentForDrawing From(
        ShapeComponent component, (IGradientBuffer, Frame)? gradientsInFrame = null)
    {
        var gradient = gradientsInFrame is var (gradients, frame)
            ? component.Color.ForDrawingWith(gradients, frame)
            : component.Color.ForDrawingWithoutGradients();

        return new ShapeComponentForDrawing(component.ZeroDistance, component.OneDistance, gradient);
    }
}

readonly record struct GradientParameters(GradientType Type, GradientId Id, GradientFlags Flags, Vector4 Parameters)
{
    public static implicit operator GradientParameters(GradientDefinition.SingleColor definition)
        => definition.Definition.ForDrawing(GradientId.None, default);
}
