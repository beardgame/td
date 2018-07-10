using Bearded.UI.Controls;
using Bearded.UI.EventArgs;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;
using MouseButtonEventArgs = Bearded.UI.EventArgs.MouseButtonEventArgs;
using MouseEventArgs = Bearded.UI.EventArgs.MouseEventArgs;

namespace Bearded.UI.Events
{
    sealed class MouseEventManager
    {
        private static readonly MouseButton[] mouseButtons =
        {
            MouseButton.Left, MouseButton.Middle, MouseButton.Right
        };

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
            var mousePosition = root.TransformViewportPosToFramePos((Vector2d) inputManager.MousePosition);

            var path = EventRouter.FindPropagationPath(
                root, control => control.IsVisible &&  control.Frame.ContainsPoint(mousePosition));

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
            
            // Mouse clicks
            foreach (var btn in mouseButtons)
            {
                var action = inputManager.Actions.Mouse.FromButton(btn);
                if (action.Hit)
                {
                    path.PropagateEvent(
                        new MouseButtonEventArgs(mousePosition, btn),
                        (c, e) => c.PreviewMouseButtonHit(e),
                        (c, e) => c.MouseButtonHit(e));
                }
                if (action.Released)
                {
                    path.PropagateEvent(
                        new MouseButtonEventArgs(mousePosition, btn),
                        (c, e) => c.PreviewMouseButtonReleased(e),
                        (c, e) => c.MouseButtonReleased(e));
                }
            }

            // Mouse scroll
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (inputManager.DeltaScrollF != 0)
            {
                path.PropagateEvent(
                    new MouseScrollEventArgs(mousePosition, inputManager.DeltaScroll, inputManager.DeltaScrollF),
                    (c, e) => c.PreviewMouseScrolled(e),
                    (c, e) => c.MouseScrolled(e));
            }

            previousPropagationPath = path;
        }
    }
}
