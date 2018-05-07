using System;

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

            // frame = math(parentFrame)

            frameNeedsUpdate = false;
        }


        public void SetAnchors(HorizontalAnchors horizontal, VerticalAnchors vertical)
        {
            horizontalAnchors = horizontal;
            verticalAnchors = vertical;

            if (frameNeedsUpdate)
                return;

            SetFrameNeedsUpdate();
        }

        public virtual void SetFrameNeedsUpdate()
        {
            frameNeedsUpdate = true;
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
    }
}
