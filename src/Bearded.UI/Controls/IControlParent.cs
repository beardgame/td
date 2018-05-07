using System.Collections.ObjectModel;

namespace Bearded.UI.Controls
{
    public interface IControlParent
    {
        Frame Frame { get; }
        ReadOnlyCollection<Control> Children { get; }
        void Add(Control child);
        void Remove(Control child);
    }
}
