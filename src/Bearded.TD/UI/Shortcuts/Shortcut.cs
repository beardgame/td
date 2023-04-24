using System;
using Bearded.UI.EventArgs;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bearded.TD.UI.Shortcuts;

readonly record struct Shortcut(Keys Key, ModifierKeys ModifierKeys, Action Action);
