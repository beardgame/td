using System;
using Bearded.UI.EventArgs;

namespace Bearded.UI.Controls
{
    public class Control
    {
        public IControlParent Parent { get; private set; }

        private Frame frame;
        private bool frameNeedsUpdate = true;

        private HorizontalAnchors horizontalAnchors;
        private VerticalAnchors verticalAnchors;

        public Frame Frame => getFrame();

        public void SetAnchors(HorizontalAnchors horizontal, VerticalAnchors vertical)
        {
            horizontalAnchors = horizontal;
            verticalAnchors = vertical;

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
                ((Anchors) horizontalAnchors).CalculateIntervalWithin(parentFrame.X),
                ((Anchors) verticalAnchors).CalculateIntervalWithin(parentFrame.Y));

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
        
        public virtual void Render()
        {
        }

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
