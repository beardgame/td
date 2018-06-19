using System;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;

namespace Bearded.UI.Controls
{
    public abstract class Control
    {
        public IControlParent Parent { get; private set; }

        private Frame frame;
        private bool frameNeedsUpdate = true;

        public HorizontalAnchors HorizontalAnchors { get; private set; } = Anchors.Default.H;
        public VerticalAnchors VerticalAnchors { get; private set; } = Anchors.Default.V;

        public Frame Frame => getFrame();

        public bool IsFocused { get; private set; }
        public bool CanBeFocused { get; protected set; }

        public void Focus()
        {
            if (!TryFocus())
                throw new InvalidOperationException("Could not focus control.");
        }

        public void Unfocus()
        {
            if (IsFocused)
                LostFocus();

            IsFocused = false;
        }

        public virtual bool TryFocus()
        {
            if (!CanBeFocused)
                return false;
            if (IsFocused)
                return true;

            IsFocused = Parent.FocusDescendant(this);

            if (IsFocused)
                Focused();

            return IsFocused;
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

        public virtual void Focused() { }
        public virtual void LostFocus() { }
    }
}
