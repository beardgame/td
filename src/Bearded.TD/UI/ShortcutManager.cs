using System;
using System.Collections.Generic;
using Bearded.UI.EventArgs;
using OpenTK.Input;

namespace Bearded.TD.UI
{
    sealed class ShortcutManager
    {
        private readonly Dictionary<Key, Action> shortcuts = new Dictionary<Key, Action>();

        public ShortcutManager RegisterShortcut(Key key, Action action)
        {
            shortcuts[key] = action;
            return this;
        }

        public void KeyHit(KeyEventArgs eventArgs)
        {
            if (shortcuts.TryGetValue(eventArgs.Key, out var action))
            {
                action();
            }
        }
    }
}
