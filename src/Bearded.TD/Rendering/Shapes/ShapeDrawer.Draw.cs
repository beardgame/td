using Bearded.Graphics.MeshBuilders;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.Rendering.Shapes.ShapeGeometry;

namespace Bearded.TD.Rendering.Shapes;

interface IShapeDrawer
{
    void DrawCircle(Vector3 xyz, float radius, ShapeGradients gradients, EdgeData edges = default);
    void DrawHexagon(Vector3 xyz, float radius, ShapeGradients gradients, float cornerRadius = 0, EdgeData edges = default);
    void DrawRectangle(Vector3 xyz, Vector2 wh, ShapeGradients gradients, float cornerRadius = 0, EdgeData edges = default);
}

sealed partial class ShapeDrawer : IShapeDrawer
{
    public void DrawHexagon(Vector3 xyz, float radius, ShapeGradients gradients, float cornerRadius = 0, EdgeData edges = default)
    {
        var (x, y, z) = xyz;
        var w = radius + edges.OuterWidth + edges.OuterGlow;
        var h = w * (1 / 0.86602540378f);
        cornerRadius = Min(radius, cornerRadius);
        var geometry = HexagonPointRadius(xyz.Xy, radius, cornerRadius, edges, 0.5f, 1);
        addQuad(x - w, x + w, y - h, y + h, z, gradients, geometry);
    }

    public void DrawCircle(Vector3 xyz, float radius, ShapeGradients gradients, EdgeData edges = default)
    {
        var (x, y, z) = xyz;
        var r = radius + edges.OuterWidth + edges.OuterGlow;
        var geometry = CirclePointRadius(xyz.Xy, radius, edges);
        addQuad(x - r, x + r, y - r, y + r, z, gradients, geometry);
    }

    public void DrawRectangle(Vector3 xyz, Vector2 wh, ShapeGradients gradients, float cornerRadius = 0, EdgeData edges = default)
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

        const float padding = 0.5f;
        topOuter -= padding;
        bottomOuter += padding;
        leftOuter -= padding;
        rightOuter += padding;

        // TODO: the optimised case below this block is currently broken if radius < inner edge width
        // to fix that, we may have to generate trapezoids or similar geometry to cover that case
        // profiling shows that this would improve gpu performance, though cpu impact has not been measured
        // hence it's unclear if that is worth it
        // though we'll eventually need more complex code like that if we want to support non rectangular shapes
        //if (colors.HasFillOrInnerGlow || wInner * hInner == 0)
        {
            addQuad(leftOuter, rightOuter, topOuter, bottomOuter, z, gradients,
                // squircleness parameters are hardcoded to subjectively most pleasing values for now
                // though setting both to 0 would be more performant where possible
                RectangleCornerSize(xyz.Xy, wh, cornerRadius, edges, 0.5f, 1));
            return;
        }

        if (outerRadius + cornerRadius > 0)
        {
            // topLeft
            addQuad(leftOuter, leftInner, topOuter, topInner, z, gradients,
                CirclePointRadius(new Vector2(leftInner, topInner), cornerRadius, edges));

            // topRight
            addQuad(rightInner, rightOuter, topOuter, topInner, z, gradients,
                CirclePointRadius(new Vector2(rightInner, topInner), cornerRadius, edges));

            // bottomLeft
            addQuad(leftOuter, leftInner, bottomInner, bottomOuter, z, gradients,
                CirclePointRadius(new Vector2(leftInner, bottomInner), cornerRadius, edges));

            // bottomRight
            addQuad(rightInner, rightOuter, bottomInner, bottomOuter, z, gradients,
                CirclePointRadius(new Vector2(rightInner, bottomInner), cornerRadius, edges));
        }

        var left = x;
        var right = x + w;
        var top = y;
        var bottom = y + h;

        if (hInner > 0)
        {
            // top
            addQuad(leftInner, rightInner, topOuter, topInner, z, gradients,
                LinePointToPoint(new Vector2(left, top), new Vector2(right, top), edges));

            // bottom
            addQuad(leftInner, rightInner, bottomInner, bottomOuter, z, gradients,
                LinePointToPoint(new Vector2(right, bottom), new Vector2(left, bottom), edges));
        }

        if (wInner > 0)
        {
            // left
            addQuad(leftOuter, leftInner, topInner, bottomInner, z, gradients,
                LinePointToPoint(new Vector2(left, bottom), new Vector2(left, top), edges));

            // right
            addQuad(rightInner, rightOuter, topInner, bottomInner, z, gradients,
                LinePointToPoint(new Vector2(right, top), new Vector2(right, bottom), edges));
        }
    }

    private void addQuad(float x0, float x1, float y0, float y1, float z, ShapeGradients gradients, ShapeGeometry geometry)
    {
        meshBuilder.AddQuad(
            new ShapeVertex(new Vector3(x0, y0, z), geometry, gradients),
            new ShapeVertex(new Vector3(x1, y0, z), geometry, gradients),
            new ShapeVertex(new Vector3(x1, y1, z), geometry, gradients),
            new ShapeVertex(new Vector3(x0, y1, z), geometry, gradients)
        );
    }
}
