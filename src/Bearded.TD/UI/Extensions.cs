using Bearded.UI.Controls;

namespace Bearded.TD.UI
{
    static class Extensions
    {
        public static AnchorTemplate MarginAllSides(this AnchorTemplate a, double margin) =>
            a.Top(margin: margin).Bottom(margin: margin).Left(margin: margin).Right(margin: margin);

        public static AnchorTemplate Below(this AnchorTemplate a, Control control, double? height = null) =>
            a.Top(margin: control.VerticalAnchors.Bottom.Offset, height: height);
    }
}
