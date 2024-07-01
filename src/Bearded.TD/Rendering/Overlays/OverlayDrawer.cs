using Bearded.Graphics;
using Bearded.TD.Game.Overlays;
using Bearded.TD.Rendering.Shapes;
using Bearded.TD.Tiles;
using Bearded.TD.UI.Shapes;
using Bearded.UI;
using Bearded.Utilities;
using Bearded.Utilities.SpaceTime;
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

        var shape = Shapes.Shapes.Hexagon(p, r);
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
        foreach (var tile in area)
        {
            Tile(color, tile, height);
        }
    }
}
