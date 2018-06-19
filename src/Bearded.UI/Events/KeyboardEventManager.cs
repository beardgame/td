using Bearded.UI.Controls;
using Bearded.Utilities.Input;

namespace Bearded.UI.Events
{
    sealed class KeyboardEventManager
    {
        private readonly RootControl root;
        private readonly InputManager inputManager;

        internal KeyboardEventManager(RootControl root, InputManager inputManager)
        {
            this.root = root;
            this.inputManager = inputManager;
        }

        internal void Update()
        {
            var focusedControl = root.FocusManager.FocusedControl;
            if (focusedControl == null) return;

            var path = EventRouter.FindPropagationPath(root, focusedControl);

            // TODO: send events down the path
        }
    }
}
