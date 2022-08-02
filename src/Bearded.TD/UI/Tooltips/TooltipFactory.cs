using Bearded.UI.Controls;

namespace Bearded.TD.UI.Tooltips;

sealed class TooltipFactory
{
    private readonly OverlayLayer overlay;

    public TooltipFactory(OverlayLayer overlay)
    {
        this.overlay = overlay;
    }

    public Tooltip ShowTooltip(TooltipDefinition definition, TooltipAnchor anchor)
    {
        var elementInOverlay =
            new CompositeControl().Anchor(_ => anchor.ToAnchorTemplate(definition.Width, definition.Height));
        elementInOverlay.Add(definition.CreateControl());
        overlay.Add(elementInOverlay);

        return new Tooltip(elementInOverlay);
    }
}
