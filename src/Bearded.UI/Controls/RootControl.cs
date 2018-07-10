using System;
using System.Collections.ObjectModel;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.UI.Controls
{
    public class RootControl : IControlParent
    {
        private readonly CompositeControl controls;

        public FocusManager FocusManager { get; }

        private Frame viewportFrame;
        public Frame Frame { get; private set; }

        public RootControl()
            : this(new CompositeControl())
        {
        }

        public RootControl(CompositeControl rootCompositeControl)
        {
            controls = rootCompositeControl;
            controls.AddTo(this);

            FocusManager = new FocusManager();
        }

        public void SetViewport(int width, int height, float uiScale)
        {
            viewportFrame = new Frame(Interval.FromStartAndSize(0, width), Interval.FromStartAndSize(0, height));
            Frame = new Frame(Interval.FromStartAndSize(0, width / uiScale), Interval.FromStartAndSize(0, height / uiScale));
            controls.SetFrameNeedsUpdate();
        }

        public Vector2d TransformViewportPosToFramePos(Vector2d viewportPos)
        {
            return new Vector2d(
                Frame.Size.X * viewportPos.X / viewportFrame.Size.X,
                Frame.Size.Y * viewportPos.Y / viewportFrame.Size.Y);
        }

        public void Render(IRendererRouter r)
        {
            if (controls.IsVisible)
            {
                controls.Render(r);
            }
        }
        
        public ReadOnlyCollection<Control> Children => controls.Children;
        public void Add(Control child) => controls.Add(child);
        public void AddOnTopOf(Control reference, Control child) => controls.AddOnTopOf(reference, child);
        public void Remove(Control child) => controls.Remove(child);

        public bool FocusDescendant(Control control)
        {
            if (!control.IsDescendantOf(this))
                throw new InvalidOperationException("Can only focus descendant.");
            
            FocusManager.Focus(control);

            return true;
        }
    }
}
