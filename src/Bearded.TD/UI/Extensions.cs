using Bearded.UI.Controls;

namespace Bearded.TD.UI
{
    static class Extensions
    {
        public static AnchorTemplate MarginAllSides(this AnchorTemplate a, double margin) =>
            a.Top(margin: margin).Bottom(margin: margin).Left(margin: margin).Right(margin: margin);

        public static AnchorTemplate Below(this AnchorTemplate a, Control control, double? height = null) =>
            a.Top(margin: control.VerticalAnchors.Bottom.Offset, height: height);

        public static AnchorTemplate Above(this AnchorTemplate a, Control control, double? height = null) =>
            a.Bottom(margin: control.VerticalAnchors.Top.Offset, height: height);

        public static AnchorTemplate RightOf(this AnchorTemplate a, Control control, double? width = null) =>
            a.Left(margin: control.HorizontalAnchors.Right.Offset, width: width);

        public static AnchorTemplate LeftOf(this AnchorTemplate a, Control control, double? width = null) =>
            a.Right(margin: control.HorizontalAnchors.Left.Offset, width: width);
    }
}
