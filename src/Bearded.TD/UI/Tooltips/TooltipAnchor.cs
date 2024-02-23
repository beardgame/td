using System;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Tooltips;

readonly record struct TooltipAnchor(Control Reference, TooltipAnchor.Direction PreferredDirection)
{
    public static TooltipAnchor RightOf(Control reference) => new(reference, Direction.Right);

    public static TooltipAnchor Above(Control reference) => new(reference, Direction.Top);

    public static TooltipAnchor LeftOf(Control reference) => new(reference, Direction.Left);

    public static TooltipAnchor Below(Control reference) => new(reference, Direction.Bottom);

    public AnchorTemplate ToAnchorTemplate(double width, double height)
    {
        switch (PreferredDirection)
        {
            case Direction.Right:
                return AnchorTemplate.Default
                    .Left(Reference.Frame.X.End + Constants.UI.Tooltip.AnchorMargin, width)
                    .Top(Reference.Frame.Y.Start, height);
            case Direction.Top:
                return AnchorTemplate.Default
                    .Left(Reference.Frame.X.Start, width)
                    .Bottom(Reference.Frame.Y.Start + Constants.UI.Tooltip.AnchorMargin, height);
            case Direction.Left:
                return AnchorTemplate.Default
                    .Right(Reference.Frame.X.Start + Constants.UI.Tooltip.AnchorMargin, width)
                    .Top(Reference.Frame.Y.Start, height);
            case Direction.Bottom:
                return AnchorTemplate.Default
                    .Left(Reference.Frame.X.Start, width)
                    .Top(Reference.Frame.Y.End + Constants.UI.Tooltip.AnchorMargin, height);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public enum Direction
    {
        Right,
        Top,
        Left,
        Bottom
    }
}
