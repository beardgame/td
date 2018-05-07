using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bearded.UI.Controls
{
    public class CompositeControl : Control, IControlParent, IEnumerable<Control>
    {
        private readonly List<Control> children = new List<Control>();

        public ReadOnlyCollection<Control> Children { get; }

        public CompositeControl()
        {
            Children = children.AsReadOnly();
        }

        public void Add(Control child)
        {
            child.AddTo(this);
            children.Add(child);
        }

        public void Remove(Control child)
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

        public override void Render()
        {
            base.Render();

            foreach (var child in children)
            {
                child.Render();
            }
        }

        public IEnumerator<Control> GetEnumerator() => children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
