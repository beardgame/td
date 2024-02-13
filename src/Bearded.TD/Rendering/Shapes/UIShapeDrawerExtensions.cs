using Bearded.Graphics;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Shapes;

static class UIShapeDrawerExtensions
{
    public static void DrawCircle(
        this IShapeDrawer drawer, Vector2d xy, double radius, ShapeColors colors, EdgeData edges = default)
        => drawer.DrawCircle(((Vector2)xy).WithZ(), (float)radius, colors, edges);

    public static void DrawCircle(
        this IShapeDrawer drawer, Vector3d xyz, double radius, ShapeColors colors, EdgeData edges = default)
        => drawer.DrawCircle((Vector3)xyz, (float)radius, colors, edges);

    public static void DrawRectangle(
        this IShapeDrawer drawer, Vector2d xy, Vector2d wh, ShapeColors colors, double cornerRadius = 0, EdgeData edges = default)
        => drawer.DrawRectangle(((Vector2)xy).WithZ(), (Vector2)wh, colors, (float)cornerRadius, edges);

    public static void DrawRectangle(
        this IShapeDrawer drawer, Vector3d xyz, Vector2d wh, ShapeColors colors, double cornerRadius = 0, EdgeData edges = default)
        => drawer.DrawRectangle((Vector3)xyz, (Vector2)wh, colors, (float)cornerRadius, edges);
}
