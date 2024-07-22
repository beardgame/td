using Bearded.Graphics;
using Bearded.TD.UI.Shapes;
using static Bearded.TD.UI.Shapes.GradientDefinition;

namespace Bearded.TD.Game.Overlays;

readonly record struct OverlayBrush(ShapeComponents Components)
{
    private static readonly Color towerRangeColor = Color.Yellow * 0.5f;
    private static readonly Color gridLineColor = Color.MediumPurple * 0.15f;

    public static OverlayBrush TowerRangeFull { get; } = new(
    [
        Glow.Outer(0.2f, towerRangeColor),
        Fill.With(ShapeColor.From(
            [
                (0, Color.Transparent),
                (0.2, towerRangeColor * 0.5f),
                (0.4, towerRangeColor * 0.5f),
                (0.6, Color.Transparent),
            ],
            Linear(
                AnchorPoint.Absolute((0, 0)),
                AnchorPoint.Absolute((0.25f, 0.25f))
            ).AddFlags(GradientFlags.Repeat)
        )),
    ]);

    public static OverlayBrush TowerRangeMinimal { get; } = new([Glow.Outer(0.2f, Color.Green * 0.25f)]);

    public static OverlayBrush BlockedTile { get; } = new([Fill.With(Color.Red * 0.5f)]);

    public static OverlayBrush TowerHighlight { get; } = new([Glow.Inner(0.2f, Color.LightCyan)]);

    public static OverlayBrush GridLines => new(
    [
        new ShapeComponent(0, -0.09f, gridLineColor),
        new ShapeComponent(-0.09f, -0.098f, SimpleGlow(gridLineColor)),
        new ShapeComponent(0, 0.5f, SimpleGlow(gridLineColor)),
    ]);
}
