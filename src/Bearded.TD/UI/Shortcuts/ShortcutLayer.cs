using System.Linq;
using Bearded.UI.EventArgs;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Shortcuts;

sealed partial class ShortcutLayer
{
    private readonly ILookup<Keys, Shortcut> shortcuts;

    private ShortcutLayer(ILookup<Keys, Shortcut> shortcuts)
    {
        this.shortcuts = shortcuts;
    }

    public bool TryHandleHit(KeyEventArgs eventArgs)
    {
        var matchedShortcut = shortcuts[eventArgs.Key].FirstOrDefault(matchesShortcut);
        if (matchedShortcut == default)
        {
            return false;
        }

        matchedShortcut.Action();
        eventArgs.Handled = true;
        return true;

        bool matchesShortcut(Shortcut shortcut) =>
            shortcut.Key == eventArgs.Key && eventArgs.ModifierKeys.IsSupersetOf(shortcut.ModifierKeys);
    }
}
