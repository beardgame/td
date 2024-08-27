using System;
using Bearded.TD.Utilities;
using Bearded.UI;
using Bearded.UI.Controls;
using Bearded.Utilities;

namespace Bearded.TD.UI;

static class Extensions
{
    public static AnchorTemplate MarginAllSides(this AnchorTemplate a, double margin) =>
        a.Top(margin: margin).Bottom(margin: margin).Left(margin: margin).Right(margin: margin);

    public static AnchorTemplate Centered(this AnchorTemplate a, double width, double height) =>
        a.HorizontallyCentered(width).VerticallyCentered(height);

    public static AnchorTemplate HorizontallyCentered(this AnchorTemplate a, double width) =>
        a.Left(-0.5 * width, width, 0.5);

    public static AnchorTemplate VerticallyCentered(this AnchorTemplate a, double height) =>
        a.Top(-0.5 * height, height, 0.5);

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

    public static AnchorTemplate TopToBottomPercentage(this AnchorTemplate template, double top, double bottom)
    {
        return template.Top(relativePercentage: top).Bottom(relativePercentage: bottom);
    }

    public static Anchor WithAddedOffset(this Anchor anchor, double offset) =>
        new Anchor(anchor.Percentage, anchor.Offset + offset);

    public static Control BindIsVisible(this Control control, IReadonlyBinding<bool> isVisible)
    {
        control.IsVisible = isVisible.Value;
        isVisible.SourceUpdated += visible => control.IsVisible = visible;
        isVisible.ControlUpdated += visible => control.IsVisible = visible;
        return control;
    }

    public static Control BindIsClickThrough(this Control control, IReadonlyBinding<bool> isClickThrough)
    {
        control.IsClickThrough = isClickThrough.Value;
        isClickThrough.SourceUpdated += clickThrough => control.IsClickThrough = clickThrough;
        isClickThrough.ControlUpdated += clickThrough => control.IsClickThrough = clickThrough;
        return control;
    }

    public static void Add(this CompositeControl parent, ReadOnlySpan<Control> children)
    {
        foreach (var child in children)
        {
            parent.Add(child);
        }
    }

    public static IControlParent? FindRoot(this Control source)
    {
        var ancestor = source.Parent;

        while (ancestor is Control c)
        {
            ancestor = c.Parent;
        }

        return ancestor;
    }

    public static double Clamped(this double value, Interval range)
    {
        return value.Clamped(range.Start, range.End);
    }
}
