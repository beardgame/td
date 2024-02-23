using System;
using Bearded.Graphics;
using Bearded.TD.Utilities;
using Bearded.Utilities;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.Rendering.Shapes.Shapes;

namespace Bearded.TD.Rendering.Shapes;

static class UIShapeDrawerExtensions
{
    public static void Draw(
        this IShapeDrawer drawer, Circle circle, ShapeColors colors, EdgeData edges = default)
        => drawer.DrawCircle((Vector3)circle.Center, (float)circle.Radius, colors, edges);

    public static void Draw(
        this IShapeDrawer drawer, Rectangle rectangle, ShapeColors colors, EdgeData edges = default)
        => drawer.DrawRectangle((Vector3)rectangle.TopLeft, (Vector2)rectangle.Size, colors, (float)rectangle.CornerRadius, edges);

    public static void DrawShadowFor(this IShapeDrawer drawer, Rectangle rectangle, Shadow shadow)
    {
        var minDimension = Min(rectangle.Size.X, rectangle.Size.Y);
        var (innerRadius, penumbra, colors) = shadowParameters(minDimension / 2, shadow);

        var minInnerSize = rectangle.Size - new Vector2d(minDimension);
        var innerSize = minInnerSize + new Vector2d(innerRadius * 2);
        var umbraToBoxEdge = minDimension / 2 - innerRadius;

        var tl = (rectangle.TopLeft.Xy + new Vector2d(umbraToBoxEdge)).WithZ(rectangle.TopLeft.Z);
        drawer.Draw(Rectangle(tl + shadow.Offset, innerSize, rectangle.CornerRadius), colors, edges: penumbra);
    }

    public static void DrawShadowFor(this IShapeDrawer drawer, Circle circle, Shadow shadow)
    {
        var (innerRadius, penumbra, colors) = shadowParameters(circle.Radius, shadow);
        drawer.Draw(Circle(circle.Center + shadow.Offset, innerRadius), colors, edges: penumbra);
    }

    private static (double innerRadius, EdgeData penumbra, ShapeColors colors) shadowParameters(double radius, Shadow shadow)
    {
        var umbraRadius = radius - shadow.PenumbraRadius;

        var (innerRadius, innerColor) = umbraRadius >= 0
            ? (umbraRadius, shadow.Color)
            : (-umbraRadius, shadow.Color * antumbraAlpha(-umbraRadius, shadow.PenumbraRadius));

        var penumbra = new EdgeData(outerGlow: (float)shadow.PenumbraRadius * 2);
        var colors = new ShapeColors(fill: innerColor, outerGlow: innerColor);
        return (innerRadius, penumbra, colors);
    }

    private static float antumbraAlpha(double antumbraRadius, double penumbraRadius)
    {
        return (float)(1 - antumbraRadius / penumbraRadius);
    }

    [Obsolete("Use Draw(Shapes.Circle()) instead")]
    public static void DrawCircle(
        this IShapeDrawer drawer, Vector2d xy, double radius, ShapeColors colors, EdgeData edges = default)
        => drawer.DrawCircle(((Vector2)xy).WithZ(), (float)radius, colors, edges);

    [Obsolete("Use Draw(Shapes.Circle()) instead")]
    public static void DrawCircle(
        this IShapeDrawer drawer, Vector3d xyz, double radius, ShapeColors colors, EdgeData edges = default)
        => drawer.DrawCircle((Vector3)xyz, (float)radius, colors, edges);

    [Obsolete("Use Draw(Shapes.Rectangle()) instead")]
    public static void DrawRectangle(
        this IShapeDrawer drawer, Vector2d xy, Vector2d wh, ShapeColors colors, double cornerRadius = 0,
        EdgeData edges = default)
        => drawer.DrawRectangle(((Vector2)xy).WithZ(), (Vector2)wh, colors, (float)cornerRadius, edges);

    [Obsolete("Use Draw(Shapes.Rectangle()) instead")]
    public static void DrawRectangle(
        this IShapeDrawer drawer, Vector3d xyz, Vector2d wh, ShapeColors colors, double cornerRadius = 0,
        EdgeData edges = default)
        => drawer.DrawRectangle((Vector3)xyz, (Vector2)wh, colors, (float)cornerRadius, edges);
}
