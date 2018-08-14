using System;
using Bearded.UI.EventArgs;
using Bearded.UI.Rendering;
using Bearded.Utilities;

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

        private bool isVisible = true;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible == value) return;
                isVisible = value;

                if (isVisible)
                {
                    MadeVisible();
                }
                else
                {
                    MadeInvisible();
                    if (IsFocused) Unfocus();
                }
            }
        }

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
            if (!CanBeFocused || !IsVisible)
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

            FrameChanged();
        }

        public void RemoveFromParent() => Parent.Remove(this);
        
        internal void AddTo(IControlParent parent)
        {
            if (Parent != null)
                throw new InvalidOperationException();

            Parent = parent;

            OnAddingToParent();
        }

        internal void RemoveFrom(IControlParent parent)
        {
            if (parent != Parent)
                throw new InvalidOperationException();

            OnRemovingFromParent();

            Parent = null;
        }
        
        public virtual void Render(IRendererRouter r)
        {
            RenderStronglyTyped(r);
        }
  
        protected abstract void RenderStronglyTyped(IRendererRouter r);

        public event GenericEventHandler<MouseEventArgs> MouseMove;
        public event GenericEventHandler<MouseEventArgs> MouseExit;
        public event GenericEventHandler<MouseButtonEventArgs> MouseButtonDown;
        public event GenericEventHandler<MouseButtonEventArgs> MouseButtonRelease;

        public virtual void PreviewMouseMoved(MouseEventArgs eventArgs) { }
        public virtual void MouseMoved(MouseEventArgs eventArgs)
        {
            MouseMove?.Invoke(eventArgs);
        }
        public virtual void PreviewMouseExited(MouseEventArgs eventArgs) { }
        public virtual void MouseExited(MouseEventArgs eventArgs)
        {
            MouseExit?.Invoke(eventArgs);
        }
        public virtual void PreviewMouseButtonHit(MouseButtonEventArgs eventArgs) { }

        public virtual void MouseButtonHit(MouseButtonEventArgs eventArgs)
        {
            MouseButtonDown?.Invoke(eventArgs);
        }
        public virtual void PreviewMouseButtonReleased(MouseButtonEventArgs eventArgs) { }

        public virtual void MouseButtonReleased(MouseButtonEventArgs eventArgs)
        {
            MouseButtonRelease?.Invoke(eventArgs);
        }
        public virtual void PreviewMouseScrolled(MouseScrollEventArgs eventArgs) { }
        public virtual void MouseScrolled(MouseScrollEventArgs eventArgs) { }

        public virtual void PreviewKeyHit(KeyEventArgs eventArgs) { }
        public virtual void KeyHit(KeyEventArgs eventArgs) { }
        public virtual void PreviewKeyReleased(KeyEventArgs eventArgs) { }
        public virtual void KeyReleased(KeyEventArgs eventArgs) { }
        public virtual void PreviewCharacterTyped(CharEventArgs eventArgs) { }
        public virtual void CharacterTyped(CharEventArgs eventArgs) { }

        protected virtual void Focused() { }
        protected virtual void LostFocus() { }

        protected virtual void FrameChanged() { }

        protected virtual void MadeVisible() { } // Not called on initialization
        protected virtual void MadeInvisible() { }
        
        protected virtual void OnAddingToParent() { }
        protected virtual void OnRemovingFromParent() { }
    }
}
