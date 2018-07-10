using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities.Input;

namespace Bearded.UI.Events
{
    sealed class KeyboardEventManager
    {
        private readonly RootControl root;
        private readonly InputManager inputManager;
        private readonly IKeyboardEventsCapturer eventsCapturer;

        internal KeyboardEventManager(
            RootControl root,
            InputManager inputManager,
            IKeyboardEventsCapturer eventsCapturer)
        {
            this.root = root;
            this.inputManager = inputManager;
            this.eventsCapturer = eventsCapturer;
        }

        internal void Update()
        {
            var focusedControl = root.FocusManager.FocusedControl;

            var path = focusedControl == null
                ? EventPropagationPath.Empty
                : EventRouter.FindPropagationPath(root, focusedControl);

            foreach (var (eventArgs, isPressed) in inputManager.KeyEvents)
            {
                var args = new KeyEventArgs(eventArgs.Key);

                if (isPressed)
                {
                    path.PropagateEvent(
                        args,
                        (c, e) => c.PreviewKeyHit(e),
                        (c, e) => c.KeyHit(e));
                    if (!args.Handled) eventsCapturer?.KeyHit(args);
                }
                else
                {
                    path.PropagateEvent(
                        args,
                        (c, e) => c.PreviewKeyReleased(e),
                        (c, e) => c.KeyReleased(e));
                    if (!args.Handled) eventsCapturer?.KeyReleased(args);
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
