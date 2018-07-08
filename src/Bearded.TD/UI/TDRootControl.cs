using Bearded.TD.UI.Layers;
using Bearded.UI.EventArgs;

namespace Bearded.TD.UI
{
    sealed class TDRootControl : DefaultRenderLayerControl
    {
        private readonly ShortcutManager shortcuts;

        public TDRootControl(ShortcutManager shortcuts)
        {
            this.shortcuts = shortcuts;
        }

        public override void KeyHit(KeyEventArgs eventArgs)
        {
            base.KeyHit(eventArgs);

            if (!eventArgs.Handled) shortcuts.KeyHit(eventArgs);
        }
    }
}
