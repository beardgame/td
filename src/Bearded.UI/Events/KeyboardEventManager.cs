using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
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

            foreach (var (eventArgs, isPressed) in inputManager.KeyEvents)
            {
                if (isPressed)
                {
                    path.PropagateEvent(
                        new KeyEventArgs(eventArgs.Key),
                        (c, e) => c.PreviewKeyHit(e),
                        (c, e) => c.KeyHit(e));
                }
                else
                {
                    path.PropagateEvent(
                        new KeyEventArgs(eventArgs.Key),
                        (c, e) => c.PreviewKeyReleased(e),
                        (c, e) => c.KeyReleased(e));
                }
            }

            foreach (var pressedChar in inputManager.PressedCharacters)
            {
                path.PropagateEvent(
                    new CharEventArgs(pressedChar),
                    (c, e) => c.PreviewCharacterTyped(e),
                    (c, e) => c.CharacterTyped(e));
            }
        }
    }
}
