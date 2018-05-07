using System.Collections.ObjectModel;

namespace Bearded.UI.Controls
{
    public class RootControl : IControlParent
    {
        private readonly CompositeControl controls = new CompositeControl();

        public Frame Frame { get; private set; }

        public RootControl(Frame frame)
        {
            Frame = frame;
            controls.AddTo(this);
        }

        public void SetFrame(Frame frame)
        {
            Frame = frame;
            controls.SetFrameNeedsUpdate();
        }
        
        public ReadOnlyCollection<Control> Children => controls.Children;
        public void Add(Control child) => controls.Add(child);
        public void Remove(Control child) => controls.Remove(child);
    }
}