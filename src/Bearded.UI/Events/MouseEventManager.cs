using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using OpenTK;

namespace Bearded.UI.Events
{
    sealed class MouseEventManager
    {
        private readonly RootControl root;

        private EventPropagationPath previousPropagationPath;

        internal MouseEventManager(RootControl root)
        {
            this.root = root;
        }

        internal void Update(Vector2d mousePosition)
        {
            var path = EventRouter.FindPropagationPath(root, control => control.Frame.ContainsPoint(mousePosition));

            path.PropagateEvent(
                new MouseEventArgs(mousePosition),
                (c, e) => c.PreviewMouseMoved(e),
                (c, e) => c.MouseMoved(e));

            if (previousPropagationPath != null)
            {
                var (removedFromPath, _) = EventPropagationPath.CalculateDeviation(previousPropagationPath, path);
                removedFromPath.PropagateEvent(
                    new MouseEventArgs(mousePosition),
                    (c, e) => c.PreviewMouseExited(e),
                    (c, e) => c.MouseExited(e));
            }

            previousPropagationPath = path;
        }
    }
}
