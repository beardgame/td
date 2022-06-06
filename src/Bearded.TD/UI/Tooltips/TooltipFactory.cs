using System;
using Bearded.UI.Controls;

namespace Bearded.TD.UI.Tooltips;

sealed class TooltipFactory
{
    private readonly OverlayLayer overlay;

    public TooltipFactory(OverlayLayer overlay)
    {
        this.overlay = overlay;
    }

    public Tooltip ShowTooltip(Func<Control> content, TooltipAnchor anchor, double width, double height)
    {
        var elementInOverlay = new CompositeControl().Anchor(_ => anchor.ToAnchorTemplate(width, height));
        elementInOverlay.Add(content());
        overlay.Add(elementInOverlay);

        return new Tooltip(elementInOverlay);
    }
}
