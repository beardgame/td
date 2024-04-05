using Bearded.TD.UI.Shapes;
using Bearded.TD.Utilities;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.UI.Shapes.GradientDefinition;

namespace Bearded.TD.Rendering.Shapes;

static class UIShapeDrawerExtensions
{
    public static void Draw(
        this IShapeDrawer drawer, Hexagon hexagon, ShapeComponentsForDrawing components)
        => drawer.DrawHexagon((Vector3)hexagon.Center, (float)hexagon.Radius, (float)hexagon.CornerRadius, components);

    public static void Draw(
        this IShapeDrawer drawer, Circle circle, ShapeComponentsForDrawing components)
        => drawer.DrawCircle((Vector3)circle.Center, (float)circle.Radius, components);

    public static void Draw(
        this IShapeDrawer drawer, Rectangle rectangle, ShapeComponentsForDrawing components)
        => drawer.drawRectangle(rectangle.TopLeft, rectangle.Size, rectangle.CornerRadius, components);

    public static void DrawShadowFor(
        this IShapeDrawer drawer, Rectangle rectangle, Shadow shadow, IShapeComponentBuffer shapeBuffer)
    {
        var minDimension = Min(rectangle.Size.X, rectangle.Size.Y);
        var (innerRadius, penumbra) = shadowParameters(minDimension / 2, shadow, shapeBuffer);

        var minInnerSize = rectangle.Size - new Vector2d(minDimension);
        var innerSize = minInnerSize + new Vector2d(innerRadius * 2);
        var umbraToBoxEdge = minDimension / 2 - innerRadius;

        var tl = (rectangle.TopLeft.Xy + new Vector2d(umbraToBoxEdge)).WithZ(rectangle.TopLeft.Z);
        drawer.drawRectangle(tl + shadow.Offset, innerSize, rectangle.CornerRadius, penumbra);
    }

    public static void DrawShadowFor(
        this IShapeDrawer drawer, Circle circle, Shadow shadow, IShapeComponentBuffer shapeBuffer)
    {
        var (innerRadius, penumbra) = shadowParameters(circle.Radius, shadow, shapeBuffer);
        drawer.drawCircle(circle.Center + shadow.Offset, innerRadius, penumbra);
    }

    public static void DrawShadowFor(
        this IShapeDrawer drawer, Hexagon hexagon, Shadow shadow, IShapeComponentBuffer shapeBuffer)
    {
        var (innerRadius, penumbra) = shadowParameters(hexagon.Radius, shadow, shapeBuffer);
        drawer.drawHexagon(hexagon.Center + shadow.Offset, innerRadius, hexagon.CornerRadius, penumbra);
    }

    private static (double innerRadius, ShapeComponentsForDrawing penumbra)
        shadowParameters(double radius, Shadow shadow, IShapeComponentBuffer shapeBuffer)
    {
        var umbraRadius = radius - shadow.PenumbraRadius;

        var (innerRadius, innerColor) = umbraRadius >= 0
            ? (umbraRadius, shadow.Color)
            : (-umbraRadius, shadow.Color * antumbraAlpha(-umbraRadius, shadow.PenumbraRadius));

        var maxDistance = (float)shadow.PenumbraRadius * 2;

        var penumbra = ShapeComponentsForDrawing.From(Glow.OuterFilled(maxDistance, innerColor), shapeBuffer);
        return (innerRadius, penumbra);
    }

    private static float antumbraAlpha(double antumbraRadius, double penumbraRadius)
    {
        return (float)(1 - antumbraRadius / penumbraRadius);
    }

    private static void drawCircle(
        this IShapeDrawer drawer, Vector3d xyz, double radius, ShapeComponentsForDrawing components)
        => drawer.DrawCircle((Vector3)xyz, (float)radius, components);

    private static void drawHexagon(
        this IShapeDrawer drawer, Vector3d xyz, double radius, double cornerRadius,
        ShapeComponentsForDrawing components)
        => drawer.DrawHexagon((Vector3)xyz, (float)radius, (float)cornerRadius, components);

    private static void drawRectangle(
        this IShapeDrawer drawer, Vector3d xyz, Vector2d wh, double cornerRadius, ShapeComponentsForDrawing components)
        => drawer.DrawRectangle((Vector3)xyz, (Vector2)wh, (float)cornerRadius, components);
}
