using System.Collections.Generic;

namespace Bearded.UI.Controls
{
    public interface IControlParent
    {
        IReadOnlyCollection<Control> Children { get; }
        Frame Frame { get; }
        void AddChild(Control child);
        void RemoveChild(Control child);
    }
}
