using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.UI.EventArgs;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Comparer = Bearded.TD.Utilities.Comparer;

namespace Bearded.TD.UI.Shortcuts;

sealed partial class ShortcutLayer
{
    public static Builder CreateBuilder() => new();

    public sealed class Builder
    {
        private static readonly IComparer<Shortcut> comparer = Comparer
            .Comparing<Shortcut, Keys>(shortcut => shortcut.Key, Comparer<Keys>.Default)
            .ThenComparing(shortcut => shortcut.ModifierKeys, Comparer.Reversed(ModifierKeysComparer.Instance));
        private readonly SortedSet<Shortcut> shortcuts;

        public Builder()
        {
            shortcuts = new SortedSet<Shortcut>(comparer);
        }

        public Builder AddShortcut(Keys key, Action action) =>
            AddShortcut(new Shortcut(key, ModifierKeys.None, action));

        public Builder AddShortcut(Keys key, ModifierKeys modifierKeys, Action action) =>
            AddShortcut(new Shortcut(key, modifierKeys, action));

        public Builder AddShortcut(Shortcut shortcut)
        {
            shortcuts.Add(shortcut);
            return this;
        }

        public ShortcutLayer Build()
        {
            return new ShortcutLayer(shortcuts.ToLookup(shortcut => shortcut.Key));
        }
    }
}
