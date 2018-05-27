namespace Bearded.UI.Controls
{
    public struct AnchorTemplate
    {
        private Anchors horizontalAnchors;
        private Anchors verticalAnchors;

        public static AnchorTemplate Default
            => new AnchorTemplate(Anchors.Default, Anchors.Default);
            
        public AnchorTemplate(Control control)
            : this(control.HorizontalAnchors, control.VerticalAnchors)
        {
        }

        public AnchorTemplate(Anchors horizontal, Anchors vertical)
        {
            horizontalAnchors = horizontal;
            verticalAnchors = vertical;
        }
            
        public void ApplyTo(Control control)
        {
            control.SetAnchors(horizontalAnchors.H, verticalAnchors.V);
        }

        public AnchorTemplate Left(double margin = 0, double? width = null, double relativePercentage = 0)
            => updateStart(ref horizontalAnchors, relativePercentage, margin, width);

        public AnchorTemplate Right(double margin = 0, double? width = null, double relativePercentage = 1)
            => updateEnd(ref horizontalAnchors, relativePercentage, margin, width);

        public AnchorTemplate Top(double margin = 0, double? height = null, double relativePercentage = 0)
            => updateStart(ref verticalAnchors, relativePercentage, margin, height);

        public AnchorTemplate Bottom(double margin = 0, double? height = null, double relativePercentage = 1)
            => updateEnd(ref verticalAnchors, relativePercentage, margin, height);

        private AnchorTemplate updateStart(ref Anchors anchors, double percentage, double margin, double? size)
        {
            anchors = new Anchors(
                new Anchor(percentage, margin),
                size.HasValue
                    ? new Anchor(percentage, margin + size.Value)
                    : anchors.End
            );
            return this;
        }

        private AnchorTemplate updateEnd(ref Anchors anchors, double percentage, double margin, double? size)
        {
            anchors = new Anchors(
                size.HasValue
                    ? new Anchor(percentage, -(margin + size.Value))
                    : anchors.Start,
                new Anchor(percentage, -margin)
            );
            return this;
        }
    }
}
