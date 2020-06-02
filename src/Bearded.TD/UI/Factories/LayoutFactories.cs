using Bearded.UI;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Factories
{
    static class LayoutFactories
    {
        public static Control WrapHorizontallyCentered(this Control control, double width)
        {
            return new CompositeControl
            {
                control.Anchor(a => a.Left(margin: -.5 * width, width: width, relativePercentage: .5))
            };
        }

        public static Control WrapVerticallyCentered(this Control control, double height)
        {
            return new CompositeControl
            {
                control.Anchor(a => a.Top(margin: -.5 * height, height: height, relativePercentage: .5))
            };
        }

        public static LayoutBuilder BuildLayout(this IControlParent parent) => new LayoutBuilder(parent);

        public sealed class LayoutBuilder
        {
            private HorizontalAnchors horizontal = new HorizontalAnchors(Anchors.Default);
            private VerticalAnchors vertical = new VerticalAnchors(Anchors.Default);

            private readonly IControlParent parent;

            public LayoutBuilder(IControlParent parent)
            {
                this.parent = parent;
            }

            public LayoutBuilder ForFullScreen()
            {
                const double m = LayoutMargin;
                horizontal = new HorizontalAnchors(new Anchors(new Anchor(0, m), new Anchor(1, -m)));
                vertical = new VerticalAnchors(new Anchors(new Anchor(0, m), new Anchor(1, -m)));
                return this;
            }

            public LayoutBuilder DockFixedSizeToTop(Control control, double height)
            {
                dockToTop(control, new Anchor(vertical.Top.Percentage, vertical.Top.Offset + height));
                return this;
            }

            public LayoutBuilder DockFractionalSizeToTop(Control control, double percentage)
            {
                dockToTop(control, new Anchor(vertical.Bottom.Percentage + percentage, vertical.Bottom.Offset));
                return this;
            }

            private void dockToTop(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(horizontal, new Anchors(vertical.Top, divider)));
                vertical = new VerticalAnchors(new Anchors(divider.WithAddedOffset(LayoutMargin), vertical.Bottom));
                parent.Add(control);
            }

            public LayoutBuilder DockFixedSizeToBottom(Control control, double height)
            {
                dockToBottom(control, new Anchor(vertical.Bottom.Percentage, vertical.Bottom.Offset - height));
                return this;
            }

            public LayoutBuilder DockFractionalSizeToBottom(Control control, double percentage)
            {
                dockToBottom(control, new Anchor(vertical.Bottom.Percentage - percentage, vertical.Bottom.Offset));
                return this;
            }

            private void dockToBottom(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(horizontal, new Anchors(divider, vertical.Bottom)));
                vertical = new VerticalAnchors(new Anchors(vertical.Top, divider.WithAddedOffset(-LayoutMargin)));
                parent.Add(control);
            }

            public LayoutBuilder DockFixedSizeToLeft(Control control, double width)
            {
                dockToLeft(control, new Anchor(horizontal.Left.Percentage, horizontal.Left.Offset + width));
                return this;
            }

            public LayoutBuilder DockFractionalSizeToLeft(Control control, double percentage)
            {
                dockToLeft(control, new Anchor(horizontal.Left.Percentage + percentage, horizontal.Left.Offset));
                return this;
            }

            private void dockToLeft(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(new Anchors(horizontal.Left, divider), vertical));
                horizontal = new HorizontalAnchors(new Anchors(divider.WithAddedOffset(LayoutMargin), horizontal.Right));
                parent.Add(control);
            }

            public LayoutBuilder DockFixedSizeToRight(Control control, double width)
            {
                dockToRight(control, new Anchor(horizontal.Right.Percentage, horizontal.Right.Offset - width));
                return this;
            }

            public LayoutBuilder DockFractionalSizeToRight(Control control, double percentage)
            {
                dockToRight(control, new Anchor(horizontal.Right.Percentage - percentage, horizontal.Right.Offset));
                return this;
            }

            private void dockToRight(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(new Anchors(divider, horizontal.Right), vertical));
                horizontal = new HorizontalAnchors(new Anchors(horizontal.Left, divider.WithAddedOffset(-LayoutMargin)));
                parent.Add(control);
            }

            public void FillContent(Control control)
            {
                control.Anchor(_ => new AnchorTemplate(horizontal, vertical));
                parent.Add(control);
            }
        }
    }
}
