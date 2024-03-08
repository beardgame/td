using Bearded.TD.UI.Shapes;
using Bearded.UI;

namespace Bearded.TD.Rendering.Shapes;

readonly struct ShapeComponentsForDrawing(ShapeComponents components, GradientDrawer? gradients, Frame frame)
{
    public EdgeData Edges
        => new(
            components.Edge.OuterWidth,
            components.Edge.InnerWidth,
            components.OuterGlow.Width,
            components.InnerGlow.Width
        );

    public ShapeGradients Gradients =>
        gradients == null
            ? new ShapeGradients(
                components.Fill.Color.ForDrawingWithoutGradients(),
                components.Edge.Color.ForDrawingWithoutGradients(),
                components.OuterGlow.Color.ForDrawingWithoutGradients(),
                components.InnerGlow.Color.ForDrawingWithoutGradients()
            )
            : new ShapeGradients(
                components.Fill.Color.ForDrawingWith(gradients, frame),
                components.Edge.Color.ForDrawingWith(gradients, frame),
                components.OuterGlow.Color.ForDrawingWith(gradients, frame),
                components.InnerGlow.Color.ForDrawingWith(gradients, frame)
            );
}
