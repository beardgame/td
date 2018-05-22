using System;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;

namespace Bearded.UI.Controls
{
    static class Test
    {
        static void test()
        {
            _ = new CompositeControl
            {
                new SimpleControl()
                    .Anchor(a => a
                        .Left(margin: 10, width: 100)
                        .Top(margin: 10, width: 20)
                    ),
                new CompositeControl
                {
                    new SimpleControl(),
                    new SimpleControl(),
                },
            };
            
        }

        public static T Anchor<T>(this T control, Func<AnchorTemplate, AnchorTemplate> build)
            where T : Control
        {
            build(new AnchorTemplate(control)).ApplyTo(control);

            return control;
        }

        public struct AnchorTemplate
        {
            private Anchors horizontalAnchors;
            private Anchors verticalAnchors;

            public AnchorTemplate(Control control)
            {
                horizontalAnchors = control.HorizontalAnchors;
                verticalAnchors = control.VerticalAnchors;
            }

            public AnchorTemplate Left(double relativePercentage = 0, double margin = 0, double? width = null)
            {
                horizontalAnchors = updateStart(horizontalAnchors, relativePercentage, margin, width);
                return this;
            }

            public AnchorTemplate Right(double relativePercentage = 0, double margin = 0, double? width = null)
            {
                horizontalAnchors = updateEnd(horizontalAnchors, relativePercentage, margin, width);
                return this;
            }

            public AnchorTemplate Top(double relativePercentage = 0, double margin = 0, double? width = null)
            {
                verticalAnchors = updateStart(verticalAnchors, relativePercentage, margin, width);
                return this;
            }

            public AnchorTemplate Bottom(double relativePercentage = 0, double margin = 0, double? width = null)
            {
                verticalAnchors = updateEnd(verticalAnchors, relativePercentage, margin, width);
                return this;
            }

            private static Anchors updateStart(Anchors anchors, double percentage, double margin, double? width)
                => new Anchors(
                    new Anchor(percentage, margin),
                    width.HasValue
                        ? new Anchor(percentage, margin + width.Value)
                        : anchors.End
                    );

            private static Anchors updateEnd(Anchors anchors, double percentage, double margin, double? width)
                => new Anchors(
                    width.HasValue
                        ? new Anchor(percentage, -(margin + width.Value))
                        : anchors.Start,
                    new Anchor(percentage, -margin)
                    );
            
            public void ApplyTo(Control control)
            {
                control.SetAnchors(horizontalAnchors.H, verticalAnchors.V);
            }
        }
    }

    public abstract class Control
    {
        public IControlParent Parent { get; private set; }

        private Frame frame;
        private bool frameNeedsUpdate = true;

        public HorizontalAnchors HorizontalAnchors { get; private set; }
        public VerticalAnchors VerticalAnchors { get; private set; }

        public Frame Frame => getFrame();

        public Control()
        {
            SetAnchors(
                new Anchors(new Anchor(0, 0), new Anchor(1, 0)).H,
                new Anchors(new Anchor(0, 0), new Anchor(1, 0)).V
                );
        }

        public void SetAnchors(HorizontalAnchors horizontal, VerticalAnchors vertical)
        {
            HorizontalAnchors = horizontal;
            VerticalAnchors = vertical;

            SetFrameNeedsUpdateIfNeeded();
        }

        public void SetFrameNeedsUpdateIfNeeded()
        {
            if (frameNeedsUpdate)
                return;

            SetFrameNeedsUpdate();
        }

        public virtual void SetFrameNeedsUpdate()
        {
            frameNeedsUpdate = true;
        }
        
        private Frame getFrame()
        {
            if (frameNeedsUpdate)
            {
                recalculateFrame();
            }

            return frame;
        }

        private void recalculateFrame()
        {
            var parentFrame = Parent.Frame;

            frame = new Frame(
                ((Anchors) HorizontalAnchors).CalculateIntervalWithin(parentFrame.X),
                ((Anchors) VerticalAnchors).CalculateIntervalWithin(parentFrame.Y));

            frameNeedsUpdate = false;
        }
        
        public void AddToParent(IControlParent parent) => parent.Add(this);

        public void RemoveFromParent() => Parent.Remove(this);
        
        internal void AddTo(IControlParent parent)
        {
            if (Parent != null)
                throw new InvalidOperationException();

            Parent = parent;
        }

        internal void RemoveFrom(IControlParent parent)
        {
            if (parent != Parent)
                throw new InvalidOperationException();

            Parent = null;
        }
        
        public virtual void Render(IRendererRouter r)
        {
            RenderStronglyTyped(r);
        }
  
        protected abstract void RenderStronglyTyped(IRendererRouter r);

        public virtual void PreviewMouseMoved(MouseEventArgs eventArgs) { }
        public virtual void MouseMoved(MouseEventArgs eventArgs) { }
        public virtual void PreviewMouseExited(MouseEventArgs eventArgs) { }
        public virtual void MouseExited(MouseEventArgs eventArgs) { }
        public virtual void PreviewMouseButtonHit(MouseButtonEventArgs eventArgs) { }
        public virtual void MouseButtonHit(MouseButtonEventArgs eventArgs) { }
        public virtual void PreviewMouseButtonReleased(MouseButtonEventArgs eventArgs) { }
        public virtual void MouseButtonReleased(MouseButtonEventArgs eventArgs) { }
    }
}
