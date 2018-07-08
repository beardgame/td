using System;
using System.Collections.Generic;
using Bearded.UI.EventArgs;
using Bearded.UI.Events;
using OpenTK.Input;

namespace Bearded.TD.UI
{
    sealed class ShortcutManager : IKeyboardEventsCapturer
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
                eventArgs.Handled = true;
            }
        }

        public void KeyReleased(KeyEventArgs eventArgs) { }
    }
}
