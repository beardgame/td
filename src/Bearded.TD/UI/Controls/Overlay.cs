using Bearded.UI.Controls;

namespace Bearded.TD.UI.Controls;

sealed class Overlay : CompositeControl
{
    public Overlay()
    {
        IsClickThrough = true;
    }
}
