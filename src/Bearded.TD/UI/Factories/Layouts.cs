using Bearded.TD.UI.Controls;
using Bearded.UI;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Factories
{
    static class Layouts
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

        public static PristineLayout BuildLayout(this IControlParent parent) =>
            new PristineLayout(parent);

        public static IColumnLayout BuildFixedColumn(this IControlParent parent) => new FixedColumnLayout(parent);

        public static IColumnLayout BuildScrollableColumn(this IControlParent parent) =>
            new ScrollableColumnLayout(parent);

        public sealed class PristineLayout : Layout
        {
            public PristineLayout(IControlParent parent) : base(parent) {}

            public PristineLayout ForFullScreen()
            {
                const double m = LayoutMargin;
                Horizontal = new HorizontalAnchors(new Anchors(new Anchor(0, m), new Anchor(1, -m)));
                Vertical = new VerticalAnchors(new Anchors(new Anchor(0, m), new Anchor(1, -m)));
                return this;
            }
        }

        public class Layout
        {
            protected HorizontalAnchors Horizontal { get; set; } = new HorizontalAnchors(Anchors.Default);
            protected VerticalAnchors Vertical { get; set; } = new VerticalAnchors(Anchors.Default);

            protected IControlParent Parent { get; }

            public Layout(IControlParent parent)
            {
                Parent = parent;
            }

            public Layout DockFixedSizeToTop(Control control, double height)
            {
                dockToTop(control, new Anchor(Vertical.Top.Percentage, Vertical.Top.Offset + height));
                return this;
            }

            public Layout DockFractionalSizeToTop(Control control, double percentage)
            {
                dockToTop(control, new Anchor(Vertical.Bottom.Percentage + percentage, Vertical.Bottom.Offset));
                return this;
            }

            private void dockToTop(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(Horizontal, new Anchors(Vertical.Top, divider)));
                Vertical = new VerticalAnchors(new Anchors(divider.WithAddedOffset(LayoutMargin), Vertical.Bottom));
                Parent.Add(control);
            }

            public Layout DockFixedSizeToBottom(Control control, double height)
            {
                dockToBottom(control, new Anchor(Vertical.Bottom.Percentage, Vertical.Bottom.Offset - height));
                return this;
            }

            public Layout DockFractionalSizeToBottom(Control control, double percentage)
            {
                dockToBottom(control, new Anchor(Vertical.Bottom.Percentage - percentage, Vertical.Bottom.Offset));
                return this;
            }

            private void dockToBottom(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(Horizontal, new Anchors(divider, Vertical.Bottom)));
                Vertical = new VerticalAnchors(new Anchors(Vertical.Top, divider.WithAddedOffset(-LayoutMargin)));
                Parent.Add(control);
            }

            public Layout DockFixedSizeToLeft(Control control, double width)
            {
                dockToLeft(control, new Anchor(Horizontal.Left.Percentage, Horizontal.Left.Offset + width));
                return this;
            }

            public Layout DockFractionalSizeToLeft(Control control, double percentage)
            {
                dockToLeft(control, new Anchor(Horizontal.Left.Percentage + percentage, Horizontal.Left.Offset));
                return this;
            }

            private void dockToLeft(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(new Anchors(Horizontal.Left, divider), Vertical));
                Horizontal = new HorizontalAnchors(new Anchors(divider.WithAddedOffset(LayoutMargin), Horizontal.Right));
                Parent.Add(control);
            }

            public Layout DockFixedSizeToRight(Control control, double width)
            {
                dockToRight(control, new Anchor(Horizontal.Right.Percentage, Horizontal.Right.Offset - width));
                return this;
            }

            public Layout DockFractionalSizeToRight(Control control, double percentage)
            {
                dockToRight(control, new Anchor(Horizontal.Right.Percentage - percentage, Horizontal.Right.Offset));
                return this;
            }

            private void dockToRight(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(new Anchors(divider, Horizontal.Right), Vertical));
                Horizontal = new HorizontalAnchors(new Anchors(Horizontal.Left, divider.WithAddedOffset(-LayoutMargin)));
                Parent.Add(control);
            }

            public void FillContent(Control control)
            {
                control.Anchor(_ => new AnchorTemplate(Horizontal, Vertical));
                Parent.Add(control);
            }
        }

        public interface IColumnLayout
        {
            IColumnLayout Add(Control control, double height);
        }

        private sealed class FixedColumnLayout : IColumnLayout
        {
            private readonly IControlParent parent;
            private double contentHeight;

            public FixedColumnLayout(IControlParent parent)
            {
                this.parent = parent;
            }

            public IColumnLayout Add(Control control, double height)
            {
                parent.Add(control.Anchor(a => a.Top(contentHeight, height)));
                contentHeight += height;
                return this;
            }
        }

        private sealed class ScrollableColumnLayout : IColumnLayout
        {
            private readonly VerticalScrollableContainer container;

            public ScrollableColumnLayout(IControlParent parent)
            {
                container = new VerticalScrollableContainer();
                parent.Add(container);
            }

            public IColumnLayout Add(Control control, double height)
            {
                container.Add(control, height);
                return this;
            }
        }
    }
}
