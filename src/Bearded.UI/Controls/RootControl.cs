using System.Collections.ObjectModel;
using Bearded.UI.Rendering;
using OpenTK;

namespace Bearded.UI.Controls
{
    public class RootControl : IControlParent
    {
        private readonly CompositeControl controls = new CompositeControl();

        private Frame viewportFrame;
        public Frame Frame { get; private set; }

        public RootControl()
        {
            controls.AddTo(this);
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
            controls.Render(r);
        }
        
        public ReadOnlyCollection<Control> Children => controls.Children;
        public void Add(Control child) => controls.Add(child);
        public void Remove(Control child) => controls.Remove(child);
    }
}
