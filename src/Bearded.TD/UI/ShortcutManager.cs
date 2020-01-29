using System;
using System.Collections.Generic;
using System.Linq;
using Bearded.TD.Utilities;
using Bearded.TD.Utilities.Collections;
using Bearded.UI.EventArgs;
using Bearded.UI.Events;
using Bearded.Utilities;
using OpenTK.Input;

namespace Bearded.TD.UI
{
    sealed class ShortcutManager : IKeyboardEventsCapturer
    {
        [Flags]
        public enum ShortcutModifierKeys : byte
        {
            None = 0,
            Shift = 1,
            Control = 1 << 1,
            Alt = 1 << 2,
            Win = 1 << 3,
        }

        private static readonly Dictionary<ShortcutModifierKeys, Func<ModifierKeys, bool>> stateExtractors =
            new Dictionary<ShortcutModifierKeys, Func<ModifierKeys, bool>>
            {
                {ShortcutModifierKeys.Shift, m => m.Shift},
                {ShortcutModifierKeys.Control, m => m.Control},
                {ShortcutModifierKeys.Alt, m => m.Alt},
                {ShortcutModifierKeys.Win, m => m.Win},
            };

        private readonly IdManager ids = new IdManager();
        private readonly MultiDictionary<Key, Shortcut> shortcuts = new MultiDictionary<Key, Shortcut>();

        public Id<Shortcut> RegisterShortcut(Key key, Action action) =>
            RegisterShortcut(key, ShortcutModifierKeys.None, action);

        public Id<Shortcut> RegisterShortcut(Key key, ShortcutModifierKeys modifierKeys, Action action)
        {
            var id = ids.GetNext<Shortcut>();
            shortcuts.Add(key, new Shortcut(id, key, modifierKeys, action));
            return id;
        }

        public bool RemoveShortcut(Key key, Id<Shortcut> id)
        {
            return shortcuts.RemoveWhere(key, s => s.Id == id) > 0;
        }

        public void KeyHit(KeyEventArgs eventArgs)
        {
            if (!shortcuts.TryGetValue(eventArgs.Key, out var found)) return;

            found.MaybeFirst(shortcut => checkModifierKeys(eventArgs, shortcut.ModifierKeys)).Match(shortcut =>
            {
                shortcut.Action();
                eventArgs.Handled = true;
            });
        }

        private static bool checkModifierKeys(KeyEventArgs eventArgs, ShortcutModifierKeys shortcutModifierKeys) =>
            stateExtractors.All(pair => (shortcutModifierKeys & pair.Key) == 0 || pair.Value(eventArgs.ModifierKeys));

        public void KeyReleased(KeyEventArgs eventArgs)
        {
        }

        public sealed class Shortcut
        {
            public Id<Shortcut> Id { get; }
            private readonly Key key;
            public ShortcutModifierKeys ModifierKeys { get; }
            public Action Action { get; }

            public Shortcut(Id<Shortcut> id, Key key, ShortcutModifierKeys modifierKeys, Action action)
            {
                Id = id;
                this.key = key;
                ModifierKeys = modifierKeys;
                Action = action;
            }
        }
    }
}
