using Bearded.TD.UI.Controls;
using Bearded.UI;
using Bearded.UI.Controls;
using static Bearded.TD.Constants.UI;

namespace Bearded.TD.UI.Factories;

static class Layouts
{
    // TODO: replace layouts with layout builders that don't finalise until done and can contain references to UI
    //       factories and other context

    public static Control WrapHorizontallyCentered(this Control control, double width)
    {
        var wrapper = CompositeControl.CreateClickThrough();
        wrapper.Add(control.Anchor(a => a.Left(margin: -.5 * width, width: width, relativePercentage: .5)));
        return wrapper;
    }

    public static Control WrapVerticallyCentered(this Control control, double height)
    {
        var wrapper = CompositeControl.CreateClickThrough();
        wrapper.Add(control.Anchor(a => a.Top(margin: -.5 * height, height: height, relativePercentage: .5)));
        return wrapper;
    }

    public static Control WrapCentered(this Control control, double width, double height)
    {
        var wrapper = CompositeControl.CreateClickThrough();
        wrapper.Add(control.Anchor(a => a
            .Left(margin: -.5 * width, width: width, relativePercentage: .5)
            .Top(margin: -.5 * height, height: height, relativePercentage: .5)));
        return wrapper;
    }

    public static Control WrapAligned(this Control control, double width, double height, double alignX, double alignY)
    {
        var wrapper = CompositeControl.CreateClickThrough();
        wrapper.Add(control.Anchor(a => a
            .Left(margin: -alignX * width, width: width, relativePercentage: alignX)
            .Top(margin: -alignY * height, height: height, relativePercentage: alignY)));
        return wrapper;
    }

    public static Control SurroundWithMargins(this Control control, double margin)
    {
        var wrapper = CompositeControl.CreateClickThrough();
        wrapper.Add(control.Anchor(a => a.MarginAllSides(margin)));
        return wrapper;
    }

    public static PristineLayout BuildLayout(this IControlParent parent) =>
        new PristineLayout(parent);

    public static FixedColumnLayout BuildFixedColumn(this IControlParent parent) => new(parent);

    public static IColumnLayout BuildScrollableColumn(this IControlParent parent) =>
        new ScrollableColumnLayout(parent);

    public static FixedRowLayout BuildFixedRow(this IControlParent parent) => new(parent);

    public sealed class PristineLayout : Layout
    {
        public PristineLayout(IControlParent parent) : base(parent) { }

        public PristineLayout ForFullScreen()
        {
            Horizontal = new HorizontalAnchors(new Anchors(new Anchor(0, LayoutMargin), new Anchor(1, -LayoutMargin)));
            Vertical = new VerticalAnchors(new Anchors(new Anchor(0, LayoutMargin), new Anchor(1, -LayoutMargin)));
            return this;
        }

        public PristineLayout ForContentBox()
        {
            // Use the same margins as full screen for now.
            return ForFullScreen();
        }

        public PristineLayout ForInnerContent()
        {
            Horizontal =
                new HorizontalAnchors(new Anchors(new Anchor(0, LayoutMarginSmall), new Anchor(1, -LayoutMarginSmall)));
            Vertical =
                new VerticalAnchors(new Anchors(new Anchor(0, LayoutMarginSmall), new Anchor(1, -LayoutMarginSmall)));
            return this;
        }
    }

    public class Layout
    {
        protected HorizontalAnchors Horizontal { get; set; } = new(Anchors.Default);
        protected VerticalAnchors Vertical { get; set; } = new(Anchors.Default);

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
            Parent.Add(control);
            moveTopAnchor(divider.WithAddedOffset(LayoutMargin));
        }

        public Layout ClearSpaceTop(double height)
        {
            moveTopAnchor(Vertical.Top.WithAddedOffset(height));
            return this;
        }

        private void moveTopAnchor(Anchor divider)
        {
            Vertical = new VerticalAnchors(new Anchors(divider, Vertical.Bottom));
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
            Parent.Add(control);
            moveBottomAnchor(divider.WithAddedOffset(-LayoutMargin));
        }

        public Layout ClearSpaceBottom(double height)
        {
            moveBottomAnchor(Vertical.Bottom.WithAddedOffset(-height));
            return this;
        }

        private void moveBottomAnchor(Anchor divider)
        {
            Vertical = new VerticalAnchors(new Anchors(Vertical.Top, divider));
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
            Parent.Add(control);
            moveLeftAnchor(divider.WithAddedOffset(LayoutMargin));
        }

        public Layout ClearSpaceLeft(double width)
        {
            moveLeftAnchor(Horizontal.Left.WithAddedOffset(width));
            return this;
        }

        private void moveLeftAnchor(Anchor divider)
        {
            Horizontal = new HorizontalAnchors(new Anchors(divider, Horizontal.Right));
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
            Parent.Add(control);
            moveRightAnchor(divider.WithAddedOffset(-LayoutMargin));
        }

        public Layout ClearSpaceRight(double width)
        {
            moveRightAnchor(Horizontal.Right.WithAddedOffset(-width));
            return this;
        }

        private void moveRightAnchor(Anchor divider)
        {
            Horizontal = new HorizontalAnchors(new Anchors(Horizontal.Left, divider));
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

    public sealed class FixedColumnLayout : IColumnLayout
    {
        private readonly IControlParent parent;

        public double Height { get; private set; }

        public FixedColumnLayout(IControlParent parent)
        {
            this.parent = parent;
        }

        public IColumnLayout Add(Control control, double height)
        {
            parent.Add(control.Anchor(a => a.Top(Height, height)));
            Height += height;
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

    public interface IRowLayout
    {
        IRowLayout AddLeft(Control control, double width);
        IRowLayout AddRight(Control control, double width);
    }

    public sealed class FixedRowLayout : IRowLayout
    {
        private readonly IControlParent parent;

        private double leftOffset;
        private double rightOffset;

        public double Width => leftOffset + rightOffset;

        public FixedRowLayout(IControlParent parent)
        {
            this.parent = parent;
        }

        public IRowLayout AddLeft(Control control, double width)
        {
            parent.Add(control.Anchor(a => a.Left(margin: leftOffset, width: width)));
            leftOffset += width;
            return this;
        }

        public IRowLayout AddRight(Control control, double width)
        {
            parent.Add(control.Anchor(a => a.Right(margin: rightOffset, width: width)));
            rightOffset += width;
            return this;
        }
    }
}
