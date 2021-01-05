using System;
using Bearded.TD.UI.Layers;
using Bearded.UI;
using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities;

namespace Bearded.TD.UI.Controls
{
    sealed class VerticalScrollableContainer : CompositeControl
    {
        private readonly CompositeControl clippingContainer = new ViewportClippingLayerControl();
        private readonly CompositeControl contentContainer = new();
        private double scrollOffset;
        private double contentHeight;

        public double ScrollOffset
        {
            get => scrollOffset;
            private set
            {
                if (scrollOffset == value) return;
                scrollOffset = value;
                clampScrollPosition();
                updateContentContainerAnchors();
            }
        }

        public VerticalScrollableContainer()
        {
            base.Add(clippingContainer);
            clippingContainer.Add(contentContainer);
        }

        public new void Add(Control child) => throw new InvalidOperationException("Need to specify a height.");

        public void Add(Control child, double height)
        {
            contentContainer.Add(child.Anchor(a => a.Top(contentHeight, height)));
            contentHeight += height;
        }

        public override void MouseScrolled(MouseScrollEventArgs eventArgs)
        {
            if (contentHeight == 0) return;

            var amountScrolled = eventArgs.DeltaScroll * -30;
            scrollOffset += amountScrolled;
            eventArgs.Handled = true;
        }

        public void ScrollToTop()
        {
            ScrollOffset = 0;
        }

        public void ScrollToBottom()
        {
            ScrollOffset = contentHeight;
        }

        protected override void FrameChanged()
        {
            base.FrameChanged();
            clampScrollPosition();
        }

        private void clampScrollPosition()
        {
            var maximumScrollOffset = Math.Max(0.0, contentHeight - clippingContainer.Frame.Size.Y);
            ScrollOffset = ScrollOffset.Clamped(0.0, maximumScrollOffset);
        }

        private void updateContentContainerAnchors()
        {
            var anchors = Anchors.Default;
            var h = anchors.H;
            anchors = new Anchors(new Anchor(0.0, -ScrollOffset), new Anchor(0.0, contentHeight - ScrollOffset));
            var v = anchors.V;
            contentContainer.SetAnchors(h, v);
        }
    }
}
