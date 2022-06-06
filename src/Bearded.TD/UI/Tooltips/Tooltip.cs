using Bearded.UI.Controls;

namespace Bearded.TD.UI.Tooltips;

sealed class Tooltip
{
    private readonly Control elementInOverlay;

    public Tooltip(Control elementInOverlay)
    {
        this.elementInOverlay = elementInOverlay;
    }

    public void Destroy()
    {
        elementInOverlay.RemoveFromParent();
    }
}
