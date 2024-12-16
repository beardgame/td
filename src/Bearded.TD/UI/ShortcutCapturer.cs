using System.Collections.Generic;
using Bearded.TD.UI.Shortcuts;
using Bearded.UI.EventArgs;
using Bearded.UI.Events;

namespace Bearded.TD.UI;

sealed class ShortcutCapturer : IKeyboardEventsCapturer
{
    private readonly List<IShortcutLayer> layers = [];

    public void AddLayer(IShortcutLayer layer)
    {
        layers.Add(layer);
    }

    public void RemoveLayer(IShortcutLayer layer)
    {
        var index = layers.IndexOf(layer);
        if (index < 0)
        {
            return;
        }
        layers.RemoveRange(index, layers.Count - index);
    }

    public void KeyHit(KeyEventArgs eventArgs)
    {
        for (var i = layers.Count - 1; i >= 0; i--)
        {
            if (layers[i].TryHandleHit(eventArgs))
            {
                return;
            }
        }
    }

    public void KeyReleased(KeyEventArgs eventArgs)
    {
    }
}
