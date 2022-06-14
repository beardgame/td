using System;
using Bearded.UI.EventArgs;

namespace Bearded.TD;

static partial class Constants
{
    public static class Input
    {
        // TODO: cannot make instances of modifier keys for comparison currently
        public static readonly Predicate<ModifierKeys> DebugForceModifier = keys => keys.Alt;
    }
}
