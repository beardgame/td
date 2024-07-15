using Bearded.Graphics;
using Bearded.TD.UI.Shapes;

namespace Bearded.TD.Game.Overlays;

readonly record struct OverlayBrush(ShapeComponents Components)
{
    public static OverlayBrush TowerRangeFull { get; } = new([Glow.Outer(0.2f, Color.Green * 0.5f)]);
    public static OverlayBrush TowerRangeMinimal { get; } = new([Glow.Outer(0.2f, Color.Green * 0.25f)]);

    public static OverlayBrush BlockedTile { get; } = new([Fill.With(Color.Red * 0.5f)]);

    public static OverlayBrush TowerHighlight { get; } = new([Glow.Inner(0.2f, Color.LightCyan)]);
}
