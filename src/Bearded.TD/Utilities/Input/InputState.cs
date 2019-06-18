using System.Collections.Generic;
using System.Linq;
using Bearded.Utilities.Input;
using OpenTK;
using OpenTK.Input;

namespace Bearded.TD.Utilities.Input
{
    class InputState
    {
        public MouseInputState Mouse { get; }
        public KeyboardInputState Keyboard { get; }

        public InputState(InputManager inputManager)
        {
            Mouse = new MouseInputState(inputManager);
            Keyboard = new KeyboardInputState(inputManager);
        }

        public ActionState ForKey(Key key)
        {
            return Keyboard.GetKeyState(key);
        }

        public ActionState ForAnyKey(params Key[] keys)
        {
            return Keyboard.GetAnyKeyState(keys);
        }

        public class MouseInputState : CapturableInputState
        {
            private readonly IAction click;
            private readonly IAction cancel;
            private readonly IAction drag;

            public ActionState Click => GetState(click);
            public ActionState Cancel => GetState(cancel);
            public ActionState Drag => GetState(drag);

            public Vector2 Position { get; }
            public float DeltaScroll { get; }

            public MouseInputState(InputManager inputManager)
            {
                click = inputManager.Actions.Mouse.LeftButton;
                cancel = inputManager.Actions.Mouse.RightButton;
                drag = inputManager.Actions.Mouse.RightButton;

                Position = inputManager.MousePosition;
                DeltaScroll = inputManager.DeltaScrollF;
            }
        }

        public class KeyboardInputState : CapturableInputState
        {
            private readonly InputManager inputManager;

            public KeyboardInputState(InputManager inputManager)
            {
                this.inputManager = inputManager;
            }

            public ActionState GetKeyState(Key key)
            {
                return GetState(inputManager.Actions.Keyboard.FromKey(key));
            }

            public ActionState GetAnyKeyState(params Key[] keys)
            {
                return GetState(InputAction.AnyOf(keys.Select(key => inputManager.Actions.Keyboard.FromKey(key))));
            }
        }

        internal abstract class CapturableInputState
        {
            protected bool IsCaptured { get; private set; }

            protected ActionState GetState(IAction action)
            {
                return IsCaptured ? new ActionState() : new ActionState(action);
            }

            public void Capture()
            {
                IsCaptured = true;
            }
        }
    }
}
