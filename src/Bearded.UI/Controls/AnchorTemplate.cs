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

        public AnchorTemplate Right(double margin = 0, double? width = null, double relativePercentage = 0)
            => updateEnd(ref horizontalAnchors, relativePercentage, margin, width);

        public AnchorTemplate Top(double margin = 0, double? width = null, double relativePercentage = 0)
            => updateStart(ref verticalAnchors, relativePercentage, margin, width);

        public AnchorTemplate Bottom(double margin = 0, double? width = null, double relativePercentage = 0)
            => updateEnd(ref verticalAnchors, relativePercentage, margin, width);

        private AnchorTemplate updateStart(ref Anchors anchors, double percentage, double margin, double? width)
        {
            anchors = new Anchors(
                new Anchor(percentage, margin),
                width.HasValue
                    ? new Anchor(percentage, margin + width.Value)
                    : anchors.End
            );
            return this;
        }

        private AnchorTemplate updateEnd(ref Anchors anchors, double percentage, double margin, double? width)
        {
            anchors = new Anchors(
                width.HasValue
                    ? new Anchor(percentage, -(margin + width.Value))
                    : anchors.Start,
                new Anchor(percentage, -margin)
            );
            return this;
        }
    }
}
