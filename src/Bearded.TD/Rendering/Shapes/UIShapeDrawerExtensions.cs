using Bearded.Graphics;
using Bearded.Utilities;
using OpenTK.Mathematics;

namespace Bearded.TD.Rendering.Shapes;

static class UIShapeDrawerExtensions
{
    public static void DrawCircle(
        this IShapeDrawer drawer, Vector2d xy, double radius, Color color, EdgeData edges = default)
        => drawer.DrawCircle(((Vector2)xy).WithZ(), (float)radius, color, edges);

    public static void DrawCircle(
        this IShapeDrawer drawer, Vector3d xyz, double radius, Color color, EdgeData edges = default)
        => drawer.DrawCircle((Vector3)xyz, (float)radius, color, edges);

    public static void DrawRectangle(
        this IShapeDrawer drawer, Vector2d xy, Vector2d wh, Color color, double cornerRadius = 0, EdgeData edges = default)
        => drawer.DrawRectangle(((Vector2)xy).WithZ(), (Vector2)wh, color, (float)cornerRadius, edges);

    public static void DrawRectangle(
        this IShapeDrawer drawer, Vector3d xyz, Vector2d wh, Color color, double cornerRadius = 0, EdgeData edges = default)
        => drawer.DrawRectangle((Vector3)xyz, (Vector2)wh, color, (float)cornerRadius, edges);
}
