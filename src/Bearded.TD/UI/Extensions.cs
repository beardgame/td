using Bearded.TD.Utilities;
using Bearded.UI;
using Bearded.UI.Controls;

namespace Bearded.TD.UI;

static class Extensions
{
    public static AnchorTemplate MarginAllSides(this AnchorTemplate a, double margin) =>
        a.Top(margin: margin).Bottom(margin: margin).Left(margin: margin).Right(margin: margin);

    public static AnchorTemplate Below(
        this AnchorTemplate a, Control control, double? height = null, double margin = 0) =>
        a.Top(margin: control.VerticalAnchors.Bottom.Offset + margin, height: height);

    public static AnchorTemplate Above(
        this AnchorTemplate a, Control control, double? height = null, double margin = 0) =>
        a.Bottom(margin: control.VerticalAnchors.Top.Offset + margin, height: height);

    public static AnchorTemplate RightOf(
        this AnchorTemplate a, Control control, double? width = null, double margin = 0) =>
        a.Left(margin: control.HorizontalAnchors.Right.Offset + margin, width: width);

    public static AnchorTemplate LeftOf(
        this AnchorTemplate a, Control control, double? width = null, double margin = 0) =>
        a.Right(margin: control.HorizontalAnchors.Left.Offset + margin, width: width);

    public static Anchor WithAddedOffset(this Anchor anchor, double offset) =>
        new Anchor(anchor.Percentage, anchor.Offset + offset);

    public static Control BindIsVisible(this Control control, Binding<bool> isVisible)
    {
        control.IsVisible = isVisible.Value;
        isVisible.SourceUpdated += visible => control.IsVisible = visible;
        return control;
    }
}