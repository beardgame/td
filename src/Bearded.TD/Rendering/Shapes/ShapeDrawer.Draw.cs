using Bearded.Graphics.MeshBuilders;
using Bearded.TD.Tiles;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.Rendering.Shapes.ShapeData;

namespace Bearded.TD.Rendering.Shapes;

interface IShapeDrawer
{
    void DrawCircle(Vector3 xyz, float radius, ShapeComponentsForDrawing components);
    void DrawHexagon(Vector3 xyz, float radius, float cornerRadius, ShapeComponentsForDrawing components);
    void DrawRectangle(Vector3 xyz, Vector2 wh, float cornerRadius, ShapeComponentsForDrawing components);
    void Draw(HexGrid grid, ShapeComponentsForDrawing components);
}

sealed partial class ShapeDrawer : IShapeDrawer
{
    public void DrawHexagon(Vector3 xyz, float radius, float cornerRadius, ShapeComponentsForDrawing components)
    {
        var (x, y, z) = xyz;
        var w = radius + components.MaxDistance;
        var h = w * (1 / 0.86602540378f);
        cornerRadius = Min(radius, cornerRadius);
        var shape = HexagonPointRadius(components.Flags, xyz.Xy, radius, cornerRadius, 0.5f, 1);
        float x1 = x + w;
        float y0 = y - h;
        float y1 = y + h;
        meshBuilder.AddRectangle(x - w, x1, y0, y1, z, components.Components, shape);
    }

    public void DrawCircle(Vector3 xyz, float radius, ShapeComponentsForDrawing components)
    {
        var (x, y, z) = xyz;
        var r = radius + components.MaxDistance;
        var geometry = CirclePointRadius(components.Flags, xyz.Xy, radius);
        float x1 = x + r;
        float y0 = y - r;
        float y1 = y + r;
        meshBuilder.AddRectangle(x - r, x1, y0, y1, z, components.Components, geometry);
    }

    public void DrawRectangle(Vector3 xyz, Vector2 wh, float cornerRadius, ShapeComponentsForDrawing components)
    {
        var (x, y, z) = xyz;
        var (w, h) = wh;

        cornerRadius = Min(cornerRadius, Min(w, h) / 2);

        var wInner = w - cornerRadius * 2;
        var hInner = h - cornerRadius * 2;

        var outerRadius = components.MaxDistance;

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


        var shape = RectangleCornerSize(components.Flags, xyz.Xy, wh, cornerRadius, 0.5f, 1);
        meshBuilder.AddRectangle(leftOuter, rightOuter, topOuter, bottomOuter, z, components.Components, shape);
    }

    static readonly Vector2 hexGridstepX = Constants.Game.World.HexagonGridUnitX.NumericValue * 3;
    static readonly Vector2 hexGridstepY = Constants.Game.World.HexagonGridUnitY.NumericValue * 3;

    public void Draw(HexGrid grid, ShapeComponentsForDrawing components)
    {
        var xy = Level.GetPosition(grid.Origin);

        var shape = HexGrid(components.Flags, grid.Origin, grid.Bits, grid.CornerRadius, 0.5f, 1);
        meshBuilder.AddParallelogram(xy.NumericValue, hexGridstepX, hexGridstepY, components, shape);
    }
}
