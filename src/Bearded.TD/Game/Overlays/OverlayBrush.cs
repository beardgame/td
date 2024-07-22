using Bearded.Graphics;
using Bearded.TD.UI.Shapes;
using static Bearded.TD.UI.Shapes.GradientDefinition;

namespace Bearded.TD.Game.Overlays;

readonly record struct OverlayBrush(ShapeComponents Components, float CornerRadius = 0.2f)
{
    private static readonly Color towerRangeColor = Color.Yellow * 0.5f;
    private static readonly Color gridLineColor = new Color(0xFF09050a) * 0.25f;

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

    public static OverlayBrush BlockedTile => new(
        new ShapeComponent(-0.042f, 0.5f, ShapeColor.From(
            [
                (0, Color.Red * 0.3f),
                (0.01, Color.Red * 0.3f),
                (0.1, Color.Transparent),
                (1, Color.Transparent),
            ], AlongEdgeNormal().WithFlags(GradientFlags.ExtendNegative)
        )), 0.15f);

    public static OverlayBrush TowerHighlight { get; } = new([Glow.Inner(0.2f, Color.LightCyan)]);

    public static OverlayBrush GridLines => new(
    [
        new ShapeComponent(-0.042f, 0.5f, ShapeColor.From(
            [
                (0, Color.Transparent),
                (0.01, gridLineColor),
                (0.1, gridLineColor),
                (1, Color.Transparent),
            ], AlongEdgeNormal()
        )),
    ], 0.08f);
}
