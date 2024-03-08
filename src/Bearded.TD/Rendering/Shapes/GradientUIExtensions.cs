using System.Diagnostics;
using Bearded.TD.UI.Shapes;

namespace Bearded.TD.Rendering.Shapes;

static class GradientUIExtensions
{
    public static ShapeComponentsForDrawing ForDrawingWith(this ShapeComponents components, GradientDrawer gradients)
        => new(components, gradients);

    public static ShapeComponentsForDrawing ForDrawingAssumingNoGradients(this ShapeComponents components)
        => new(components, null);

    public static GradientParameters ForDrawingWith(this ShapeColor color, GradientDrawer gradients)
    {
        var gradientId = color.Gradient.IsDefaultOrEmpty
            ? GradientId.None
            : gradients.AddGradient(color.Gradient.AsSpan());
        return color.Definition.ToGradientParameters(gradientId);
    }

    public static GradientParameters ForDrawingWithoutGradients(this ShapeColor color)
        => color.Definition.ToGradientParameters(GradientId.None);

    public static GradientParameters ToGradientParameters(this GradientDefinition definition,  GradientId gradientId)
    {
        validateGradientParameters(definition, gradientId);

        return new GradientParameters(definition.Type, gradientId, definition.Parameters);
    }

    [Conditional("DEBUG")]
    private static void validateGradientParameters(GradientDefinition definition, GradientId gradientId)
    {
        var expectsGradient = definition.Type >= GradientType.Linear;
        var gotGradient = !gradientId.IsNone;

        Debug.Assert(expectsGradient == gotGradient,
            expectsGradient
                ? $"Gradient ID required for gradient type {definition.Type}."
                : $"Gradient ID not allowed for gradient type {definition.Type}."
        );
    }
}
