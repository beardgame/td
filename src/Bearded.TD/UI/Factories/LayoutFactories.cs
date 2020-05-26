using Bearded.UI;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Factories
{
    static class LayoutFactories
    {
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

            public void DockFixedSizeToTop(Control control, double height)
            {
                dockToTop(control, new Anchor(vertical.Top.Percentage, vertical.Top.Offset + height));
            }

            public void DockFractionalSizeToTop(Control control, double percentage)
            {
                dockToTop(control, new Anchor(vertical.Bottom.Percentage + percentage, vertical.Bottom.Offset));
            }

            private void dockToTop(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(horizontal, new Anchors(vertical.Top, divider)));
                vertical = new VerticalAnchors(new Anchors(divider, vertical.Bottom));
                parent.Add(control);
            }

            public void DockFixedSizeToBottom(Control control, double height)
            {
                dockToBottom(control, new Anchor(vertical.Bottom.Percentage, vertical.Bottom.Offset - height));
            }

            public void DockFractionalSizeToBottom(Control control, double percentage)
            {
                dockToBottom(control, new Anchor(vertical.Bottom.Percentage - percentage, vertical.Bottom.Offset));
            }

            private void dockToBottom(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(horizontal, new Anchors(divider, vertical.Bottom)));
                vertical = new VerticalAnchors(new Anchors(vertical.Top, divider));
                parent.Add(control);
            }

            public void DockFixedSizeToLeft(Control control, double width)
            {
                dockToLeft(control, new Anchor(horizontal.Left.Percentage, horizontal.Left.Offset + width));
            }

            public void DockFractionalSizeToLeft(Control control, double percentage)
            {
                dockToLeft(control, new Anchor(horizontal.Left.Percentage + percentage, horizontal.Left.Offset));
            }

            private void dockToLeft(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(new Anchors(horizontal.Left, divider), vertical));
                horizontal = new HorizontalAnchors(new Anchors(divider, horizontal.Right));
                parent.Add(control);
            }

            public void DockFixedSizeToRight(Control control, double width)
            {
                dockToRight(control, new Anchor(horizontal.Right.Percentage, horizontal.Right.Offset - width));
            }

            public void DockFractionalSizeToRight(Control control, double percentage)
            {
                dockToRight(control, new Anchor(horizontal.Right.Percentage - percentage, horizontal.Right.Offset));
            }

            private void dockToRight(Control control, Anchor divider)
            {
                control.Anchor(_ => new AnchorTemplate(new Anchors(divider, horizontal.Right), vertical));
                horizontal = new HorizontalAnchors(new Anchors(horizontal.Left, divider));
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
