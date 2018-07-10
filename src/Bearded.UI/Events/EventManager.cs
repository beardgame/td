using Bearded.UI.Controls;
using Bearded.Utilities.Input;

namespace Bearded.UI.Events
{
    public sealed class EventManager
    {
        private readonly MouseEventManager mouseEvents;
        private readonly KeyboardEventManager keyboardEvents;

        public EventManager(
            RootControl root,
            InputManager inputManager) : this(root, inputManager, null) { }

        public EventManager(
            RootControl root,
            InputManager inputManager,
            IKeyboardEventsCapturer keyboardEventsCapturer)
        {
            mouseEvents = new MouseEventManager(root, inputManager);
            keyboardEvents = new KeyboardEventManager(root, inputManager, keyboardEventsCapturer);
        }

        public void Update()
        {
            mouseEvents.Update();
            keyboardEvents.Update();
        }
    }
}
