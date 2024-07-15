using Bearded.TD.Tiles;
using OpenTK.Mathematics;
using static System.Math;
using static Bearded.TD.Rendering.Shapes.ShapeData;

namespace Bearded.TD.Rendering.Shapes;

interface IShapeDrawer
{
    void Draw(Circle circle, ShapeComponentsForDrawing components);
    void Draw(Hexagon hexagon, ShapeComponentsForDrawing components);
    void Draw(Rectangle rectangle, ShapeComponentsForDrawing components);
    void Draw(HexGrid grid, ShapeComponentsForDrawing components);
}

sealed partial class ShapeDrawer : IShapeDrawer
{
    private const float padding = 0.5f;

    public void Draw(Circle circle, ShapeComponentsForDrawing components)
    {
        var xyz = (Vector3)circle.Center;
        var radius = (float)circle.Radius;
        var (x, y, z) = xyz;

        var r = radius + components.MaxDistance + padding;

        var geometry = CirclePointRadius(components.Flags, xyz.Xy, radius);
        meshBuilder.AddRectangle(x - r, x + r, y - r, y + r, z, components.Components, geometry);
    }

    public void Draw(Hexagon hexagon, ShapeComponentsForDrawing components)
    {
        var xyz = (Vector3)hexagon.Center;
        var radius = (float)hexagon.Radius;
        var (x, y, z) = xyz;

        var w = radius + components.MaxDistance;
        var h = w * (1 / 0.86602540378f) + padding;
        w += padding;

        var cornerRadius = Min(radius, (float)hexagon.CornerRadius);
        var shape = HexagonPointRadius(components.Flags, xyz.Xy, radius, cornerRadius, 0.5f, 1);

        meshBuilder.AddRectangle(x - w, x + w, y - h, y + h, z, components.Components, shape);
    }

    public void Draw(Rectangle rectangle, ShapeComponentsForDrawing components)
    {
        var xyz = (Vector3)rectangle.TopLeft;
        var wh = (Vector2)rectangle.Size;
        var (x, y, z) = xyz;
        var (w, h) = wh;

        var r = components.MaxDistance + padding;

        var cornerRadius = Min((float)rectangle.CornerRadius, Min(w, h) / 2);
        var shape = RectangleCornerSize(components.Flags, xyz.Xy, wh, cornerRadius, 0.5f, 1);

        meshBuilder.AddRectangle(x - r, x + w + r, y - r, y + h + r, z, components.Components, shape);
    }

    private static readonly Vector2 hexGridStepX = Constants.Game.World.HexagonGridUnitX.NumericValue * 3;
    private static readonly Vector2 hexGridStepY = Constants.Game.World.HexagonGridUnitY.NumericValue * 3;

    public void Draw(HexGrid grid, ShapeComponentsForDrawing components)
    {
        var xy = Level.GetPosition(grid.Origin);

        var shape = HexGrid(components.Flags, grid.Origin, grid.Bits, grid.CornerRadius, 0.5f, 1);
        meshBuilder.AddParallelogram(xy.NumericValue, hexGridStepX, hexGridStepY, components, shape);
    }
}
