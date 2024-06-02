using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

namespace Bearded.TD.Game.Overlays;

sealed class ActiveOverlays
{
    readonly record struct ActiveOverlay(int Id, IOverlayLayer Layer);

    private readonly FrozenDictionary<DrawOrder, List<ActiveOverlay>> overlays = Enum
        .GetValues<DrawOrder>()
        .ToFrozenDictionary(o => o, _ => new List<ActiveOverlay>());

    private int lastId;

    public Controller Add(IOverlayLayer overlay)
    {
        var activeOverlay = new ActiveOverlay(++lastId, overlay);
        overlays[overlay.DrawOrder].Add(activeOverlay);

        return new Controller(this, overlay.DrawOrder, activeOverlay.Id);
    }

    public IEnumerable<IOverlayLayer> LayersFor(DrawOrder drawOrder)
        => overlays[drawOrder].Select(o => o.Layer);

    public readonly struct Controller(ActiveOverlays overlays, DrawOrder drawOrder, int id)
        : IDisposable
    {
        public void Hide()
        {
            var i = id;
            overlays.overlays[drawOrder].RemoveAll(o => o.Id == i);
        }

        void IDisposable.Dispose() => Hide();
    }
}
