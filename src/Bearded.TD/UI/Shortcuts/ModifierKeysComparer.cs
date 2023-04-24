using System.Collections.Generic;
using Bearded.UI.EventArgs;

namespace Bearded.TD.UI.Shortcuts;

sealed class ModifierKeysComparer : IComparer<ModifierKeys>
{
    public static ModifierKeysComparer Instance { get; } = new();

    private ModifierKeysComparer() {}

    public int Compare(ModifierKeys left, ModifierKeys right)
    {
        return specificity(left).CompareTo(specificity(right));
    }

    private static int specificity(ModifierKeys modifierKeys)
    {
        var shift = modifierKeys.Shift ? 1 : 0;
        var alt = modifierKeys.Alt ? 1 : 0;
        var control = modifierKeys.Control ? 1 : 0;
        var win = modifierKeys.Win ? 1 : 0;
        return shift + alt + control + win;
    }
}
