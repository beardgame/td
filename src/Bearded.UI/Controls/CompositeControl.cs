using System.Collections.Generic;

namespace Bearded.UI.Controls
{
    public class CompositeControl : Control, IControlParent
    {
        private readonly List<Control> children;

        public IReadOnlyCollection<Control> Children { get; }

        public void AddChild(Control child)
        {
            child.AddTo(this);
            children.Add(child);
        }

        public void RemoveChild(Control child)
        {
            child.RemoveFrom(this);
            children.Remove(child);
        }

        public override void SetFrameNeedsUpdate()
        {
            base.SetFrameNeedsUpdate();

            foreach (var child in children)
            {
                child.SetFrameNeedsUpdate();
            }
        }
    }
}
