using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bearded.TD.Game.Overlays;

interface IActiveOverlay
{
    void Deactivate();
}

readonly record struct MaskedOverlay(IOverlayLayer Layer, IOverlayMask? Mask);

sealed class ActiveOverlays
{
    public static ImmutableArray<DrawOrder> DrawOrdersInOrder { get; } = [..Enum.GetValues<DrawOrder>()];

    private sealed class ActiveOverlay(ActiveOverlays overlays, IOverlayLayer layer, IOverlayMask? mask) : IActiveOverlay
    {
        public IOverlayLayer Layer { get; } = layer;
        public IOverlayMask? Mask { get; } = mask;

        public void Deactivate()
        {
            overlays.overlays[Layer.DrawOrder].Remove(this);
        }
    }

    private readonly FrozenDictionary<DrawOrder, List<ActiveOverlay>> overlays =
        DrawOrdersInOrder.ToFrozenDictionary(o => o, _ => new List<ActiveOverlay>());

    public IActiveOverlay Activate(IOverlayLayer overlay, IOverlayMask? mask = null)
    {
        var activeOverlay = new ActiveOverlay(this, overlay, mask);
        overlays[overlay.DrawOrder].Add(activeOverlay);

        return activeOverlay;
    }

    public IEnumerable<MaskedOverlay> LayersFor(DrawOrder drawOrder)
        => overlays[drawOrder].Select(o => new MaskedOverlay(o.Layer, o.Mask));
}
