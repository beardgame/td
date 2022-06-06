using Bearded.UI.Controls;

namespace Bearded.TD.UI;

sealed class OverlayLayer
{
    private readonly IControlParent overlayLayerControl;

    public OverlayLayer(IControlParent overlayLayerControl)
    {
        this.overlayLayerControl = overlayLayerControl;
    }

    public void Add(Control control)
    {
        overlayLayerControl.Add(control);
    }
}
