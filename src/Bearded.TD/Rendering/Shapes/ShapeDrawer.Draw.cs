using Bearded.Graphics;
using Bearded.Graphics.MeshBuilders;
using OpenTK.Mathematics;
using static System.Math;

namespace Bearded.TD.Rendering.Shapes;

interface IShapeDrawer
{
    void DrawCircle(Vector3 xyz, float radius, Color color, EdgeData edges = default);
    void DrawRectangle(Vector3 xyz, Vector2 wh, Color color, float cornerRadius = 0, EdgeData edges = default);
}

sealed partial class ShapeDrawer : IShapeDrawer
{
    public void DrawCircle(Vector3 xyz, float radius, Color color, EdgeData edges = default)
    {
        var (x, y, z) = xyz;
        var r = radius + edges.OuterWidth + edges.OuterGlow;
        var geometry = ShapeGeometry.CirclePointRadius(xyz.Xy, radius, edges);
        addQuad(x - r, x + r, y - r, y + r, z, color, geometry);
    }

    public void DrawRectangle(Vector3 xyz, Vector2 wh, Color color, float cornerRadius = 0, EdgeData edges = default)
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

        { // topLeft
            var geometry = ShapeGeometry.CirclePointRadius(
                new Vector2(leftInner, topInner), cornerRadius, edges);
            addQuad(leftOuter, leftInner, topOuter, topInner, z, color, geometry);
        }
        { // topRight
            var geometry = ShapeGeometry.CirclePointRadius(
                new Vector2(rightInner, topInner), cornerRadius, edges);
            addQuad(rightInner, rightOuter, topOuter, topInner, z, color, geometry);
        }
        { // bottomLeft
            var geometry = ShapeGeometry.CirclePointRadius(
                new Vector2(leftInner, bottomInner), cornerRadius, edges);
            addQuad(leftOuter, leftInner, bottomInner, bottomOuter, z, color, geometry);
        }
        { // bottomRight
            var geometry = ShapeGeometry.CirclePointRadius(
                new Vector2(rightInner, bottomInner), cornerRadius, edges);
            addQuad(rightInner, rightOuter, bottomInner, bottomOuter, z, color, geometry);
        }

        var left = x;
        var right = x + w;
        var top = y;
        var bottom = y + h;

        { // top
            var geometry = ShapeGeometry.LinePointToPoint(
                new Vector2(left, top), new Vector2(right, top), edges);
            addQuad(leftInner, rightInner, topOuter, topInner, z, color, geometry);
        }
        { // bottom
            var geometry = ShapeGeometry.LinePointToPoint(
                new Vector2(right, bottom), new Vector2(left, bottom), edges);
            addQuad(leftInner, rightInner, bottomInner, bottomOuter, z, color, geometry);
        }
        { // left
            var geometry = ShapeGeometry.LinePointToPoint(
                new Vector2(left, bottom), new Vector2(left, top), edges);
            addQuad(leftOuter, leftInner, topInner, bottomInner, z, color, geometry);
        }
        { // right
            var geometry = ShapeGeometry.LinePointToPoint(
                new Vector2(right, top), new Vector2(right, bottom), edges);
            addQuad(rightInner, rightOuter, topInner, bottomInner, z, color, geometry);
        }

        { // middle
            var geometry = ShapeGeometry.Fill();
            addQuad(leftInner, rightInner, topInner, bottomInner, z, color, geometry);
        }
    }

    private void addQuad(float x0, float x1, float y0, float y1, float z, Color color, ShapeGeometry geometry)
    {
        meshBuilder.AddQuad(
            new ShapeVertex(new Vector3(x0, y0, z), color, geometry),
            new ShapeVertex(new Vector3(x1, y0, z), color, geometry),
            new ShapeVertex(new Vector3(x1, y1, z), color, geometry),
            new ShapeVertex(new Vector3(x0, y1, z), color, geometry)
        );
    }
}
