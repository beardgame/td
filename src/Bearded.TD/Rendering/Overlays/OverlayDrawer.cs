using System;
using Bearded.Graphics;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Shapes;
using Bearded.UI;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
using static Bearded.TD.Rendering.Shapes.Shapes;
using static Bearded.UI.Interval;

namespace Bearded.TD.Rendering.Overlays;

sealed class OverlayDrawer(ShapeDrawer shapes, ComponentBuffer componentBuffer, GradientBuffer gradients)
    : IOverlayDrawer
{
    public void Tile(Color color, Tile tile, Unit height = default)
    {
        var p = Level.GetPosition(tile).NumericValue.WithZ(height.NumericValue);

        const float w = Constants.Game.World.HexagonWidth;
        const float r = w * 0.5f;

        var shape = Hexagon(p, r, 0.2f);
        var frame = new Frame(FromStartAndSize(p.X - r, w), FromStartAndSize(p.Y - r, w));

        var componentsForDrawing = ShapeComponentsForDrawing.From(
            [Fill.With(color)],
            componentBuffer,
            (gradients, frame),
            ShapeFlags.ProjectOnDepthBuffer
        );

        shapes.Draw(shape, componentsForDrawing);
    }

    public void Area(Color color, IArea area, Unit height = default)
    {
        var minX = int.MaxValue;
        var minY = int.MaxValue;
        var maxX = int.MinValue;
        var maxY = int.MinValue;

        foreach (var tile in area)
        {
            minX = Math.Min(minX, tile.X);
            minY = Math.Min(minY, tile.Y);
            maxX = Math.Max(maxX, tile.X);
            maxY = Math.Max(maxY, tile.Y);
        }

        if (minX == int.MaxValue)
            return;

        var frame = new Frame(FromStartAndSize(0, 1), FromStartAndSize(0, 1));
        var componentsForDrawing = ShapeComponentsForDrawing.From(
            [Glow.Outer(0.2f, color)],
            componentBuffer,
            (gradients, frame),
            ShapeFlags.ProjectOnDepthBuffer
        );

        var cellCountX = (maxX - minX) / 3 + 1;
        var cellCountY = (maxY - minY) / 3 + 1;

        var firstOrigin = new Tile(minX - 1, minY - 1);

        for (var y = 0; y < cellCountY; y++)
        {
            for (var x = 0; x < cellCountX; x++)
            {
                var origin = firstOrigin + new Step(x * 3, y * 3);

                var bits = HexGridBitField.From(origin, area.Contains);

                if (bits.IsEmpty)
                    continue;

                shapes.Draw(HexGrid(origin, bits, 0.2f), componentsForDrawing);
            }
        }
    }
}
