using Bearded.UI.EventArgs;

namespace Bearded.TD.UI.Shortcuts;

interface IShortcutLayer
{
    bool TryHandleHit(KeyEventArgs eventArgs);
}
