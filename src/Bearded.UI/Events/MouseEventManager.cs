using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities.Input;
using OpenTK;

namespace Bearded.UI.Events
{
    sealed class MouseEventManager
    {
        private readonly RootControl root;
        private readonly InputManager inputManager;

        private EventPropagationPath previousPropagationPath;

        internal MouseEventManager(RootControl root, InputManager inputManager)
        {
            this.root = root;
            this.inputManager = inputManager;
        }

        internal void Update()
        {
            var mousePosition = (Vector2d) inputManager.MousePosition;

            var path = EventRouter.FindPropagationPath(root, control => control.Frame.ContainsPoint(mousePosition));

            // Mouse move
            path.PropagateEvent(
                new MouseEventArgs(mousePosition),
                (c, e) => c.PreviewMouseMoved(e),
                (c, e) => c.MouseMoved(e));

            // Mouse exit
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
