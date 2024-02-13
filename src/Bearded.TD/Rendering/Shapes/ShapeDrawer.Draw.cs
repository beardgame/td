using Bearded.Graphics.MeshBuilders;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.Rendering.Shapes.ShapeGeometry;

namespace Bearded.TD.Rendering.Shapes;

interface IShapeDrawer
{
    void DrawCircle(Vector3 xyz, float radius, ShapeColors colors, EdgeData edges = default);
    void DrawRectangle(Vector3 xyz, Vector2 wh, ShapeColors colors, float cornerRadius = 0, EdgeData edges = default);
}

sealed partial class ShapeDrawer : IShapeDrawer
{
    public void DrawCircle(Vector3 xyz, float radius, ShapeColors colors, EdgeData edges = default)
    {
        var (x, y, z) = xyz;
        var r = radius + edges.OuterWidth + edges.OuterGlow;
        var geometry = CirclePointRadius(xyz.Xy, radius, edges);
        addQuad(x - r, x + r, y - r, y + r, z, colors, geometry);
    }

    public void DrawRectangle(Vector3 xyz, Vector2 wh, ShapeColors colors, float cornerRadius = 0, EdgeData edges = default)
    {
        var (x, y, z) = xyz;
        var (w, h) = wh;

        cornerRadius = Min(cornerRadius, Min(w, h) / 2);

        var wInner = w - cornerRadius * 2;
        var hInner = h - cornerRadius * 2;

        var outerRadius = edges.OuterWidth + edges.OuterGlow;

        var leftOuter = x - outerRadius;
        var leftInner = x + cornerRadius;
        var rightInner = leftInner + wInner;
        var rightOuter = rightInner + cornerRadius + outerRadius;

        var topOuter = y - outerRadius;
        var topInner = y + cornerRadius;
        var bottomInner = topInner + hInner;
        var bottomOuter = bottomInner + cornerRadius + outerRadius;

        if (outerRadius + cornerRadius > 0)
        {
            // topLeft
            addQuad(leftOuter, leftInner, topOuter, topInner, z, colors,
                CirclePointRadius(new Vector2(leftInner, topInner), cornerRadius, edges));

            // topRight
            addQuad(rightInner, rightOuter, topOuter, topInner, z, colors,
                CirclePointRadius(new Vector2(rightInner, topInner), cornerRadius, edges));

            // bottomLeft
            addQuad(leftOuter, leftInner, bottomInner, bottomOuter, z, colors,
                CirclePointRadius(new Vector2(leftInner, bottomInner), cornerRadius, edges));

            // bottomRight
            addQuad(rightInner, rightOuter, bottomInner, bottomOuter, z, colors,
                CirclePointRadius(new Vector2(rightInner, bottomInner), cornerRadius, edges));
        }

        var left = x;
        var right = x + w;
        var top = y;
        var bottom = y + h;

        if (hInner > 0)
        {
            // top
            addQuad(leftInner, rightInner, topOuter, topInner, z, colors,
                LinePointToPoint(new Vector2(left, top), new Vector2(right, top), edges));

            // bottom
            addQuad(leftInner, rightInner, bottomInner, bottomOuter, z, colors,
                LinePointToPoint(new Vector2(right, bottom), new Vector2(left, bottom), edges));
        }

        if (wInner > 0)
        {
            // left
            addQuad(leftOuter, leftInner, topInner, bottomInner, z, colors,
                LinePointToPoint(new Vector2(left, bottom), new Vector2(left, top), edges));

            // right
            addQuad(rightInner, rightOuter, topInner, bottomInner, z, colors,
                LinePointToPoint(new Vector2(right, top), new Vector2(right, bottom), edges));
        }

        if (colors.HasFill)
        { // middle
            addQuad(leftInner, rightInner, topInner, bottomInner, z, colors, Fill());
        }
    }

    private void addQuad(float x0, float x1, float y0, float y1, float z, ShapeColors colors, ShapeGeometry geometry)
    {
        meshBuilder.AddQuad(
            new ShapeVertex(new Vector3(x0, y0, z), geometry, colors),
            new ShapeVertex(new Vector3(x1, y0, z), geometry, colors),
            new ShapeVertex(new Vector3(x1, y1, z), geometry, colors),
            new ShapeVertex(new Vector3(x0, y1, z), geometry, colors)
        );
    }
}
