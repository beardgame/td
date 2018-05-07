using Bearded.UI.Controls;
using OpenTK;

namespace Bearded.UI.Events
{
    public sealed class EventManager
    {
        private readonly MouseEventManager mouseEvents;

        public EventManager(RootControl root)
        {
            mouseEvents = new MouseEventManager(root);
        }

        public void Update()
        {
            mouseEvents.Update(Vector2d.Zero);
        }
    }
}
