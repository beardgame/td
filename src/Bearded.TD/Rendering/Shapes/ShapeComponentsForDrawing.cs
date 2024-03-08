using Bearded.TD.UI.Shapes;

namespace Bearded.TD.Rendering.Shapes;

readonly struct ShapeComponentsForDrawing(ShapeComponents components, GradientDrawer? gradients)
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
                components.Fill.Color.ForDrawingWith(gradients),
                components.Edge.Color.ForDrawingWith(gradients),
                components.OuterGlow.Color.ForDrawingWith(gradients),
                components.InnerGlow.Color.ForDrawingWith(gradients)
            );
}
