using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using OpenTK;

namespace Bearded.UI.Events
{
    sealed class MouseEventManager
    {
        private readonly RootControl root;

        internal MouseEventManager(RootControl root)
        {
            this.root = root;
        }

        internal void Update(Vector2d mousePosition)
        {
            var path = EventRouter.FindPropagationPath(root, control => control.Frame.ContainsPoint(mousePosition));
            if (path.IsEmpty) return;

            path.PropagateEvent(
                new MouseEventArgs(mousePosition),
                (c, e) => c.PreviewMouseMoved(e),
                (c, e) => c.MouseMoved(e));
        }
    }
}
