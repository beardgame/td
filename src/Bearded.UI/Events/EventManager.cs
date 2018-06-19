using Bearded.UI.Controls;
using Bearded.Utilities.Input;

namespace Bearded.UI.Events
{
    public sealed class EventManager
    {
        private readonly MouseEventManager mouseEvents;
        private readonly KeyboardEventManager keyboardEvents;

        public EventManager(RootControl root, InputManager inputManager)
        {
            mouseEvents = new MouseEventManager(root, inputManager);
            keyboardEvents = new KeyboardEventManager(root, inputManager);
        }

        public void Update()
        {
            mouseEvents.Update();
            keyboardEvents.Update();
        }
    }
}
