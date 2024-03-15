using Bearded.TD.Utilities;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.UI.Shapes.GradientDefinition;

namespace Bearded.TD.Rendering.Shapes;

static class UIShapeDrawerExtensions
{
    public static void Draw(
        this IShapeDrawer drawer, Hexagon hexagon, ShapeComponentsForDrawing components)
        => drawer.DrawHexagon((Vector3)hexagon.Center, (float)hexagon.Radius, components.Gradients,
            (float)hexagon.CornerRadius, components.Edges);

    public static void Draw(
        this IShapeDrawer drawer, Circle circle, ShapeComponentsForDrawing components)
        => drawer.DrawCircle((Vector3)circle.Center, (float)circle.Radius, components.Gradients, components.Edges);

    public static void Draw(
        this IShapeDrawer drawer, Rectangle rectangle, ShapeComponentsForDrawing components)
        => drawer.drawRectangle(
            rectangle.TopLeft, rectangle.Size,
            components.Gradients, rectangle.CornerRadius, components.Edges);

    public static void DrawShadowFor(this IShapeDrawer drawer, Rectangle rectangle, Shadow shadow)
    {
        var minDimension = Min(rectangle.Size.X, rectangle.Size.Y);
        var (innerRadius, penumbra, colors) = shadowParameters(minDimension / 2, shadow);

        var minInnerSize = rectangle.Size - new Vector2d(minDimension);
        var innerSize = minInnerSize + new Vector2d(innerRadius * 2);
        var umbraToBoxEdge = minDimension / 2 - innerRadius;

        var tl = (rectangle.TopLeft.Xy + new Vector2d(umbraToBoxEdge)).WithZ(rectangle.TopLeft.Z);
        drawer.drawRectangle(tl + shadow.Offset, innerSize, colors, rectangle.CornerRadius, penumbra);
    }

    public static void DrawShadowFor(this IShapeDrawer drawer, Circle circle, Shadow shadow)
    {
        var (innerRadius, penumbra, colors) = shadowParameters(circle.Radius, shadow);
        drawer.drawCircle(circle.Center + shadow.Offset, innerRadius, colors, edges: penumbra);
    }

    public static void DrawShadowFor(this IShapeDrawer drawer, Hexagon hexagon, Shadow shadow)
    {
        var (innerRadius, penumbra, colors) = shadowParameters(hexagon.Radius, shadow);
        drawer.drawHexagon(hexagon.Center + shadow.Offset, innerRadius, colors, hexagon.CornerRadius, edges: penumbra);
    }

    private static (double innerRadius, EdgeData penumbra, ShapeGradients colors)
        shadowParameters(double radius, Shadow shadow)
    {
        var umbraRadius = radius - shadow.PenumbraRadius;

        var (innerRadius, innerColor) = umbraRadius >= 0
            ? (umbraRadius, shadow.Color)
            : (-umbraRadius, shadow.Color * antumbraAlpha(-umbraRadius, shadow.PenumbraRadius));

        var penumbra = new EdgeData(outerGlow: (float)shadow.PenumbraRadius * 2);
        var colors = new ShapeGradients(fill: Constant(innerColor), outerGlow: SimpleGlow(innerColor));
        return (innerRadius, penumbra, colors);
    }

    private static float antumbraAlpha(double antumbraRadius, double penumbraRadius)
    {
        return (float)(1 - antumbraRadius / penumbraRadius);
    }

    private static void drawCircle(
        this IShapeDrawer drawer, Vector3d xyz, double radius, ShapeGradients gradients, EdgeData edges = default)
        => drawer.DrawCircle((Vector3)xyz, (float)radius, gradients, edges);

    private static void drawHexagon(
        this IShapeDrawer drawer, Vector3d xyz, double radius, ShapeGradients gradients, double cornerRadius = 0,
        EdgeData edges = default)
        => drawer.DrawHexagon((Vector3)xyz, (float)radius, gradients, (float)cornerRadius, edges);

    private static void drawRectangle(
        this IShapeDrawer drawer, Vector3d xyz, Vector2d wh, ShapeGradients gradients, double cornerRadius = 0,
        EdgeData edges = default)
        => drawer.DrawRectangle((Vector3)xyz, (Vector2)wh, gradients, (float)cornerRadius, edges);
}
