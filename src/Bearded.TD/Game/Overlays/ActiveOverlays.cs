﻿using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Bearded.TD.Game.Overlays;

interface IActiveOverlay
{
    void Deactivate();
}

sealed class ActiveOverlays
{
    public static ImmutableArray<DrawOrder> DrawOrdersInOrder { get; } = [..Enum.GetValues<DrawOrder>()];

    private sealed class ActiveOverlay(ActiveOverlays overlays, IOverlayLayer layer) : IActiveOverlay
    {
        public IOverlayLayer Layer { get; } = layer;

        public void Deactivate()
        {
            overlays.overlays[Layer.DrawOrder].Remove(this);
        }
    }

    private readonly FrozenDictionary<DrawOrder, List<ActiveOverlay>> overlays =
        DrawOrdersInOrder.ToFrozenDictionary(o => o, _ => new List<ActiveOverlay>());

    public IActiveOverlay Activate(IOverlayLayer overlay)
    {
        var activeOverlay = new ActiveOverlay(this, overlay);
        overlays[overlay.DrawOrder].Add(activeOverlay);

        return activeOverlay;
    }

    public IEnumerable<IOverlayLayer> LayersFor(DrawOrder drawOrder)
        => overlays[drawOrder].Select(o => o.Layer);
}
